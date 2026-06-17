using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Services;

public class OrganizationService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly NotificationService _notificationService;

    public OrganizationService(IDbContextFactory<AppDbContext> contextFactory, UserManager<IdentityUser> userManager, NotificationService notificationService)
    {
        _contextFactory = contextFactory;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    private AppDbContext CreateContext() => _contextFactory.CreateDbContext();

    // ── ORGANIZAÇÕES ────────────────────────────────────────────

    public async Task<List<Organization>> GetUserOrganizationsAsync(string userId)
    {
        using var _context = CreateContext();
        return await _context.OrganizationMembers
            .Where(m => m.UserId == userId)
            .Include(m => m.Organization)
                .ThenInclude(o => o.Members)
            .Select(m => m.Organization)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Organization?> GetOrganizationByIdAsync(int id)
    {
        using var _context = CreateContext();
        return await _context.Organizations
            .Include(o => o.Members)
            .Include(o => o.Invites)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Organization>> SearchOrganizationsAsync(string query)
    {
        using var _context = CreateContext();
        return await _context.Organizations
            .Where(o => o.AllowJoinRequests && o.Name.Contains(query))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Organization> CreateOrganizationAsync(Organization org, string ownerUserId, string ownerUserName)
    {
        using var _context = CreateContext();
        org.OwnerId = ownerUserId;
        org.CreatedAt = DateTime.UtcNow;
        _context.Organizations.Add(org);
        await _context.SaveChangesAsync();

        _context.OrganizationMembers.Add(new OrganizationMember
        {
            OrganizationId = org.Id,
            UserId = ownerUserId,
            UserName = ownerUserName,
            Role = MemberRole.Owner,
            JoinedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return org;
    }

    public async Task UpdateOrganizationAsync(Organization org)
    {
        using var _context = CreateContext();
        var existing = await _context.Organizations.FindAsync(org.Id);
        if (existing == null) return;

        existing.Name = org.Name;
        existing.Description = org.Description;
        existing.AllowJoinRequests = org.AllowJoinRequests;
        existing.TaskCreationPolicy = org.TaskCreationPolicy;
        existing.TaskEditPolicy = org.TaskEditPolicy;
        existing.TaskDeletePolicy = org.TaskDeletePolicy;
        existing.TaskCompletePolicy = org.TaskCompletePolicy;
        existing.KanbanViewPolicy = org.KanbanViewPolicy;
        existing.KanbanViewOthersPolicy = org.KanbanViewOthersPolicy;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteOrganizationAsync(int orgId)
    {
        using var _context = CreateContext();
        var org = await _context.Organizations.FindAsync(orgId);
        if (org == null) return;
        _context.Organizations.Remove(org);
        await _context.SaveChangesAsync();
    }

    // ── MEMBROS ─────────────────────────────────────────────────

    public async Task<MemberRole?> GetUserRoleAsync(int orgId, string userId)
    {
        using var _context = CreateContext();
        var member = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == orgId && m.UserId == userId);
        return member?.Role;
    }

    public async Task<bool> IsMemberAsync(int orgId, string userId)
    {
        using var _context = CreateContext();
        return await _context.OrganizationMembers
            .AnyAsync(m => m.OrganizationId == orgId && m.UserId == userId);
    }

    public async Task SetMemberRoleAsync(int orgId, string userId, MemberRole newRole)
    {
        using var _context = CreateContext();
        var member = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == orgId && m.UserId == userId);
        if (member == null) return;
        member.Role = newRole;
        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(int orgId, string userId)
    {
        using var _context = CreateContext();
        var member = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == orgId && m.UserId == userId);
        if (member == null) return;
        _context.OrganizationMembers.Remove(member);
        await _context.SaveChangesAsync();

        var org = await _context.Organizations.FindAsync(orgId);
        await _notificationService.CreateAsync(
            userId: userId,
            message: $"Você foi removido da organização \"{org?.Name}\"",
            type: NotificationType.OrgMemberRemoved,
            link: "/organizations"
        );
    }

    public async Task TransferOwnershipAsync(int orgId, string currentOwnerId, string newOwnerId)
    {
        using var _context = CreateContext();
        var org = await _context.Organizations.FindAsync(orgId);
        if (org == null || org.OwnerId != currentOwnerId) return;

        var oldOwner = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == orgId && m.UserId == currentOwnerId);
        var newOwner = await _context.OrganizationMembers
            .FirstOrDefaultAsync(m => m.OrganizationId == orgId && m.UserId == newOwnerId);

        if (oldOwner != null) oldOwner.Role = MemberRole.Admin;
        if (newOwner != null) newOwner.Role = MemberRole.Owner;
        org.OwnerId = newOwnerId;

        await _context.SaveChangesAsync();
    }

    // ── CONVITES E SOLICITAÇÕES ──────────────────────────────────

    public async Task<List<OrganizationInvite>> GetPendingInvitesForUserAsync(string userId)
    {
        using var _context = CreateContext();
        return await _context.OrganizationInvites
            .Include(i => i.Organization)
            .Where(i => i.TargetUserId == userId && i.Status == InviteStatus.Pending)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<OrganizationInvite>> GetPendingRequestsForOrgAsync(int orgId)
    {
        using var _context = CreateContext();
        return await _context.OrganizationInvites
            .Where(i => i.OrganizationId == orgId && i.Status == InviteStatus.Pending
                        && i.InitiatedByUserId == i.TargetUserId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task SendInviteAsync(int orgId, string targetUserId, string targetUserName,
                                      string invitedByUserId, string invitedByUserName)
    {
        using var _context = CreateContext();
        var exists = await _context.OrganizationInvites.AnyAsync(i =>
            i.OrganizationId == orgId && i.TargetUserId == targetUserId &&
            i.Status == InviteStatus.Pending);
        if (exists) return;

        var org = await _context.Organizations.FindAsync(orgId);

        _context.OrganizationInvites.Add(new OrganizationInvite
        {
            OrganizationId = orgId,
            TargetUserId = targetUserId,
            TargetUserName = targetUserName,
            InitiatedByUserId = invitedByUserId,
            InitiatedByUserName = invitedByUserName,
            Status = InviteStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        await _notificationService.CreateAsync(
            userId: targetUserId,
            message: $"{invitedByUserName} convidou você para a organização \"{org?.Name}\"",
            type: NotificationType.OrgInviteReceived,
            link: "/organizations"
        );
    }

    public async Task RequestJoinAsync(int orgId, string userId, string userName)
    {
        using var _context = CreateContext();
        var org = await _context.Organizations.FindAsync(orgId);
        if (org == null || !org.AllowJoinRequests) return;

        var exists = await _context.OrganizationInvites.AnyAsync(i =>
            i.OrganizationId == orgId && i.TargetUserId == userId &&
            i.Status == InviteStatus.Pending);
        if (exists) return;

        _context.OrganizationInvites.Add(new OrganizationInvite
        {
            OrganizationId = orgId,
            TargetUserId = userId,
            TargetUserName = userName,
            InitiatedByUserId = userId,
            InitiatedByUserName = userName,
            Status = InviteStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        await _notificationService.CreateAsync(
            userId: org.OwnerId,
            message: $"{userName} solicitou entrada na organização \"{org.Name}\"",
            type: NotificationType.OrgApplicationReceived,
            link: $"/org/{orgId}"
        );
    }

    public async Task ResolveInviteAsync(int inviteId, bool accept, string resolvedByUserId)
    {
        using var _context = CreateContext();
        var invite = await _context.OrganizationInvites.FindAsync(inviteId);
        if (invite == null || invite.Status != InviteStatus.Pending) return;

        var org = await _context.Organizations.FindAsync(invite.OrganizationId);
        var isApplication = invite.InitiatedByUserId == invite.TargetUserId;

        invite.Status = accept ? InviteStatus.Accepted : InviteStatus.Rejected;
        invite.ResolvedAt = DateTime.UtcNow;

        if (accept)
        {
            var alreadyMember = await _context.OrganizationMembers
                .AnyAsync(m => m.OrganizationId == invite.OrganizationId && m.UserId == invite.TargetUserId);
            if (!alreadyMember)
            {
                _context.OrganizationMembers.Add(new OrganizationMember
                {
                    OrganizationId = invite.OrganizationId,
                    UserId = invite.TargetUserId,
                    UserName = invite.TargetUserName,
                    Role = MemberRole.Member,
                    JoinedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        if (isApplication)
        {
            await _notificationService.CreateAsync(
                userId: invite.TargetUserId,
                message: accept
                    ? $"Sua solicitação para \"{org?.Name}\" foi aceita!"
                    : $"Sua solicitação para \"{org?.Name}\" foi recusada.",
                type: accept ? NotificationType.OrgApplicationAccepted : NotificationType.OrgApplicationDeclined,
                link: accept ? "/organizations" : null
            );
        }
        else
        {
            await _notificationService.CreateAsync(
                userId: invite.InitiatedByUserId,
                message: accept
                    ? $"{invite.TargetUserName} aceitou o convite para \"{org?.Name}\""
                    : $"{invite.TargetUserName} recusou o convite para \"{org?.Name}\"",
                type: accept ? NotificationType.OrgInviteAccepted : NotificationType.OrgInviteDeclined,
                link: $"/org/{invite.OrganizationId}"
            );
        }
    }

    // ── HELPERS (sem acesso a banco — permanecem iguais) ──────────

    public bool CanCreateTask(Organization org, MemberRole role)
    {
        return org.TaskCreationPolicy switch
        {
            TaskCreationPolicy.OwnerOnly => role == MemberRole.Owner,
            TaskCreationPolicy.AdminsAndOwner => role is MemberRole.Owner or MemberRole.Admin,
            TaskCreationPolicy.AllMembers => true,
            _ => false
        };
    }

    public bool CanEditTask(Organization org, MemberRole role)
    {
        return org.TaskEditPolicy switch
        {
            TaskEditPolicy.OwnerOnly => role == MemberRole.Owner,
            TaskEditPolicy.AdminsAndOwner => role is MemberRole.Owner or MemberRole.Admin,
            TaskEditPolicy.AllMembers => true,
            _ => false
        };
    }

    public bool CanDeleteTask(Organization org, MemberRole role)
    {
        return org.TaskDeletePolicy switch
        {
            TaskDeletePolicy.OwnerOnly => role == MemberRole.Owner,
            TaskDeletePolicy.AdminsAndOwner => role is MemberRole.Owner or MemberRole.Admin,
            TaskDeletePolicy.AllMembers => true,
            _ => false
        };
    }

    public bool CanCompleteTask(Organization org, MemberRole role)
    {
        return org.TaskCompletePolicy switch
        {
            TaskCompletePolicy.OwnerOnly => role == MemberRole.Owner,
            TaskCompletePolicy.AdminsAndOwner => role is MemberRole.Owner or MemberRole.Admin,
            TaskCompletePolicy.AllMembers => true,
            _ => false
        };
    }

    public bool CanViewKanban(Organization org, MemberRole role)
    {
        return org.KanbanViewPolicy switch
        {
            KanbanViewPolicy.OwnerOnly      => role == MemberRole.Owner,
            KanbanViewPolicy.AdminsAndOwner => role is MemberRole.Owner or MemberRole.Admin,
            KanbanViewPolicy.AllMembers     => true,
            _ => false
        };
    }

    public bool CanViewOthersKanban(Organization org, MemberRole role)
    {
        return org.KanbanViewOthersPolicy switch
        {
            KanbanViewOthersPolicy.OwnerOnly      => role == MemberRole.Owner,
            KanbanViewOthersPolicy.AdminsAndOwner => role is MemberRole.Owner or MemberRole.Admin,
            KanbanViewOthersPolicy.AllMembers     => true,
            _ => false
        };
    }

    public async Task<List<IdentityUser>> SearchUsersAsync(string query)
    {
        return await _userManager.Users
            .Where(u => u.UserName != null && u.UserName.Contains(query))
            .Take(10)
            .ToListAsync();
    }
}