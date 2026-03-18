using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Services;

public class NotificationService(AppDbContext db)
{
    public async Task CreateAsync(string userId, string message, NotificationType type, string? link = null)
    {
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
        return await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Notification>> GetAllAsync(string userId)
    {
        return await db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await db.Notifications.FindAsync(notificationId);
        if (notification is null) return;

        notification.IsRead = true;
        await db.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var unread = await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        unread.ForEach(n => n.IsRead = true);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int notificationId)
    {
        var notification = await db.Notifications.FindAsync(notificationId);
        if (notification is null) return;
        db.Notifications.Remove(notification);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAllAsync(string userId)
    {
        var all = await db.Notifications
            .Where(n => n.UserId == userId)
            .ToListAsync();
        db.Notifications.RemoveRange(all);
        await db.SaveChangesAsync();
    }

}
