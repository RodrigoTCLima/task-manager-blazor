using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Services;

public class NotificationService(IDbContextFactory<AppDbContext> contextFactory)
{
    private AppDbContext CreateContext() => contextFactory.CreateDbContext();

    public async Task CreateAsync(string userId, string message, NotificationType type, string? link = null)
    {
        using var db = CreateContext();
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            Type = type,
            Link = link,
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetUnreadAsync(string userId)
    {
        using var db = CreateContext();
        return await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetAllAsync(string userId)
    {
        using var db = CreateContext();
        return await db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        using var db = CreateContext();
        var notification = await db.Notifications.FindAsync(notificationId);
        if (notification is null) return;

        notification.IsRead = true;
        await db.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        using var db = CreateContext();
        var unread = await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        unread.ForEach(n => n.IsRead = true);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int notificationId)
    {
        using var db = CreateContext();
        var notification = await db.Notifications.FindAsync(notificationId);
        if (notification is null) return;
        db.Notifications.Remove(notification);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAllAsync(string userId)
    {
        using var db = CreateContext();
        var all = await db.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync();
        db.Notifications.RemoveRange(all);
        await db.SaveChangesAsync();
    }
}