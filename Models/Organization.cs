using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models;

public enum TaskCreationPolicy
{
    OwnerOnly,
    AdminsAndOwner,
    AllMembers
}

public class Organization
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string OwnerId { get; set; } = string.Empty; // IdentityUser Id

    public bool AllowJoinRequests { get; set; } = false;

    public TaskCreationPolicy TaskCreationPolicy { get; set; } = TaskCreationPolicy.OwnerOnly;

    public List<OrganizationMember> Members { get; set; } = new();
    public List<OrganizationInvite> Invites { get; set; } = new();
}
