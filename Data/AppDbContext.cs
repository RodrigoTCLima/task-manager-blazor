using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models;

namespace TaskManager.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions options) : base(options) { }

    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<OrganizationMember> OrganizationMembers { get; set; }
    public DbSet<OrganizationInvite> OrganizationInvites { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Comment -> TaskItem
        builder.Entity<Comment>()
            .HasOne<TaskItem>()
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Garante que colunas JSON nunca sejam NULL no banco
        builder.Entity<TaskItem>()
            .Property(t => t.Tags)
            .HasDefaultValueSql("'[]'");

        builder.Entity<TaskItem>()
            .Property(t => t.DependencyOnTaskIds)
            .HasDefaultValueSql("'[]'");

        builder.Entity<TaskItem>()
            .Property(t => t.ReviewByUserId)
            .HasDefaultValueSql("'[]'");

        builder.Entity<TaskItem>()
            .Property(t => t.ReviewedByUserId)
            .HasDefaultValueSql("'[]'");

        // OrganizationMember -> Organization
        builder.Entity<OrganizationMember>()
            .HasOne(m => m.Organization)
            .WithMany(o => o.Members)
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrganizationInvite -> Organization
        builder.Entity<OrganizationInvite>()
            .HasOne(i => i.Organization)
            .WithMany(o => o.Invites)
            .HasForeignKey(i => i.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Serializar listas como JSON no SQLite
        builder.Entity<TaskItem>()
            .Property(t => t.Tags)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
            );

        builder.Entity<TaskItem>()
            .Property(t => t.AssignedToUserIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
            );
    }
}
