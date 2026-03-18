using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models;

public class Notification
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public string? Link { get; set; }

    public NotificationType Type { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum NotificationType
{
    OrgInviteReceived,
    OrgApplicationReceived,
    OrgInviteAccepted,
    OrgInviteDeclined,
    OrgApplicationAccepted,
    OrgApplicationDeclined,
    TaskAssigned,
    TaskReviewPending,
    OrgMemberRemoved,
    TaskCompleted,
    TaskReviewed,
    TaskCommented,
}
