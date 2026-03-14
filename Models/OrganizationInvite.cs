using System;

namespace TaskManager.Models;

public enum InviteStatus
{
    Pending,
    Accepted,
    Rejected,
    JoinRequest // solicitação do usuário, não convite
}

public class OrganizationInvite
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public string TargetUserId { get; set; } = string.Empty;   // quem foi convidado ou fez pedido
    public string TargetUserName { get; set; } = string.Empty;

    public string InitiatedByUserId { get; set; } = string.Empty; // quem convidou ou o próprio usuário
    public string InitiatedByUserName { get; set; } = string.Empty;

    public InviteStatus Status { get; set; } = InviteStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
