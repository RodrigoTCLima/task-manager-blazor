using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var isPostgres = Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";
        var emptyJson  = isPostgres ? "'[]'::text" : "'[]'";
        var jsonOptions = (System.Text.Json.JsonSerializerOptions?)null;

        // ── Value comparers for List<string> and List<int> ──────────────────
        var stringListComparer = new ValueComparer<List<string>>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
            v => v.ToList()
        );

        var nullableStringListComparer = new ValueComparer<List<string>?>(
            (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
            v => v == null ? 0 : v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
            v => v == null ? null : v.ToList()
        );

        var intListComparer = new ValueComparer<List<int>?>(
            (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
            v => v == null ? 0 : v.Aggregate(0, (h, i) => HashCode.Combine(h, i.GetHashCode())),
            v => v == null ? null : v.ToList()
        );

        // ── Comment -> TaskItem ──────────────────────────────────────────────
        builder.Entity<Comment>()
            .HasOne<TaskItem>()
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Tags ─────────────────────────────────────────────────────────────
        builder.Entity<TaskItem>()
            .Property(t => t.Tags)
            .HasDefaultValueSql(emptyJson)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, jsonOptions),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>()
            )
            .Metadata.SetValueComparer(stringListComparer);

        // ── AssignedToUserIds ─────────────────────────────────────────────────
        builder.Entity<TaskItem>()
            .Property(t => t.AssignedToUserIds)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, jsonOptions),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>()
            )
            .Metadata.SetValueComparer(stringListComparer);

        // ── DependencyOnTaskIds ───────────────────────────────────────────────
        builder.Entity<TaskItem>()
            .Property(t => t.DependencyOnTaskIds)
            .HasDefaultValueSql(emptyJson)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v ?? new List<int>(), jsonOptions),
                v => System.Text.Json.JsonSerializer.Deserialize<List<int>>(v, jsonOptions) ?? new List<int>()
            )
            .Metadata.SetValueComparer(intListComparer);

        // ── ReviewByUserId ────────────────────────────────────────────────────
        builder.Entity<TaskItem>()
            .Property(t => t.ReviewByUserId)
            .HasDefaultValueSql(emptyJson)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v ?? new List<string>(), jsonOptions),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>()
            )
            .Metadata.SetValueComparer(nullableStringListComparer);

        // ── ReviewedByUserId ──────────────────────────────────────────────────
        builder.Entity<TaskItem>()
            .Property(t => t.ReviewedByUserId)
            .HasDefaultValueSql(emptyJson)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v ?? new List<string>(), jsonOptions),
                v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>()
            )
            .Metadata.SetValueComparer(nullableStringListComparer);

        // ── OrganizationMember -> Organization ────────────────────────────────
        builder.Entity<OrganizationMember>()
            .HasOne(m => m.Organization)
            .WithMany(o => o.Members)
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── OrganizationInvite -> Organization ────────────────────────────────
        builder.Entity<OrganizationInvite>()
            .HasOne(i => i.Organization)
            .WithMany(o => o.Invites)
            .HasForeignKey(i => i.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
