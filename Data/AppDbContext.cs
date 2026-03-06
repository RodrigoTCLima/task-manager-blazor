using Microsoft.EntityFrameworkCore;
using TaskManager.Models;

namespace TaskManager.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // Configura a relação entre TaskItem e Comment
        modelBuilder.Entity<Comment>()
            .HasOne<TaskItem>()
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
