using System;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models;

public enum MemberRole
{
    Owner,
    Admin,
    Member
}

public class OrganizationMember
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public string UserId { get; set; } = string.Empty; // IdentityUser Id
    public string UserName { get; set; } = string.Empty; // Cache do nome para exibição

    public MemberRole Role { get; set; } = MemberRole.Member;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
