using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.DTOs;

namespace TaskManager.Services;

public class TaskService
{
    private readonly AppDbContext _context;
    private readonly NotificationService _notificationService;
    private readonly IServiceProvider _serviceProvider;

    public TaskService(AppDbContext context, NotificationService notificationService, IServiceProvider serviceProvider)
    {
        _context = context;
        _notificationService = notificationService;
        _serviceProvider = serviceProvider;
    }

    // Resolve OrganizationService lazily to avoid circular DI
    private OrganizationService OrgService =>
        _serviceProvider.GetRequiredService<OrganizationService>();
    public async Task<List<TaskItem>> GetAllTasksAsync(string? userId = null, int? orgId = null)
    {
        IQueryable<TaskItem> query = _context.Tasks
            .Include(t => t.Comments)
            .AsNoTracking();

        if (orgId.HasValue)
        {
            query = query.Where(t => t.OrganizationId == orgId.Value);
        }
        else if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.AuthorUserId == userId
                                  && (t.OrganizationId == null || t.OrganizationId == 0));
        }

        return await query
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetTaskByIdAsync(int id, string? userId = null)
    {
        return await _context.Tasks
            .Include(t => t.Comments)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task UpdateTaskAsync(TaskItem task, string? requestingUserId = null)
    {
        var existing = await _context.Tasks.FindAsync(task.Id);
        if (existing == null) return;

        // Backend permission guard
        if (requestingUserId != null)
        {
            if (existing.OrganizationId.HasValue)
            {
                var org = await OrgService.GetOrganizationByIdAsync(existing.OrganizationId.Value);
                var role = await OrgService.GetUserRoleAsync(existing.OrganizationId.Value, requestingUserId);
                if (org == null || !role.HasValue || !OrgService.CanEditTask(org, role.Value))
                    return; // silently block
            }
            else if (existing.AuthorUserId != requestingUserId)
            {
                return; // only author can edit personal tasks
            }
        }

        var existingReviewedCount = existing.ReviewedByUserId?.Count ?? 0;
        var newReviewedCount = task.ReviewedByUserId?.Count ?? 0;
        var wasReviewed = existingReviewedCount == 0 && newReviewedCount > 0;

        existing.Title = task.Title;
        existing.Description = task.Description;
        existing.Priority = task.Priority;
        existing.Category = task.Category;
        existing.DueDate = task.DueDate;
        existing.HasAlarm = task.HasAlarm;
        existing.AlarmTime = task.AlarmTime;
        existing.IsRecurrent = task.IsRecurrent;
        existing.RecurrencePattern = task.RecurrencePattern;
        existing.IsCompleted = task.IsCompleted;
        existing.Tags = task.Tags;
        existing.DependencyOnTaskIds = task.DependencyOnTaskIds;
        existing.AssignedToUserIds = task.AssignedToUserIds;
        existing.OrganizationId = task.OrganizationId;
        existing.NeedsReview = task.NeedsReview;
        existing.ReviewByUserId = task.ReviewByUserId;
        existing.ReviewedByUserId = task.ReviewedByUserId;

        await _context.SaveChangesAsync();

        // Notifica responsáveis quando a task for revisada/aprovada
        if (wasReviewed && task.AssignedToUserIds != null)
        {
            foreach (var uid in task.AssignedToUserIds)
            {
                await _notificationService.CreateAsync(
                    userId: uid,
                    message: $"A tarefa \"{task.Title}\" foi revisada e aprovada",
                    type: NotificationType.TaskReviewed,
                    link: $"/task/{task.Id}"
                );
            }
        }
    }


    public async Task DeleteTaskAsync(int id, string? requestingUserId = null)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return;

        // Backend permission guard
        if (requestingUserId != null)
        {
            if (task.OrganizationId.HasValue)
            {
                var org = await OrgService.GetOrganizationByIdAsync(task.OrganizationId.Value);
                var role = await OrgService.GetUserRoleAsync(task.OrganizationId.Value, requestingUserId);
                if (org == null || !role.HasValue || !OrgService.CanDeleteTask(org, role.Value))
                    return; // silently block
            }
            else if (task.AuthorUserId != requestingUserId)
            {
                return; // only author can delete personal tasks
            }
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    // Listar comentários de uma tarefa
    public async Task<List<Comment>> GetCommentsAsync(int taskId)
    {
        return await _context.Comments
            .Where(c => c.TaskItemId == taskId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    // Adicionar comentário
    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var task = await _context.Tasks.FindAsync(comment.TaskItemId);
        if (task != null)
        {
            var toNotify = new HashSet<string>();

            if (!string.IsNullOrEmpty(task.AuthorUserId))
                toNotify.Add(task.AuthorUserId);

            if (task.AssignedToUserIds != null)
                foreach (var uid in task.AssignedToUserIds)
                    toNotify.Add(uid);

            toNotify.Remove(comment.AuthorUserId); // não notifica quem comentou

            foreach (var uid in toNotify)
            {
                await _notificationService.CreateAsync(
                    userId: uid,
                    message: $"Novo comentário na tarefa \"{task.Title}\"",
                    type: NotificationType.TaskCommented,
                    link: $"/task/{task.Id}"
                );
            }
        }

        return comment;
    }


    // Deletar comentário
    public async Task DeleteCommentAsync(int commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ToggleCompleteAsync(int id, string completedByUserId)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return;

        // Backend permission guard — assignees always allowed regardless of policy
        var isAssignee = task.AssignedToUserIds?.Contains(completedByUserId) == true;
        var isAuthor   = task.AuthorUserId == completedByUserId;

        if (!isAssignee && !isAuthor)
        {
            if (task.OrganizationId.HasValue)
            {
                var org  = await OrgService.GetOrganizationByIdAsync(task.OrganizationId.Value);
                var role = await OrgService.GetUserRoleAsync(task.OrganizationId.Value, completedByUserId);
                if (org == null || !role.HasValue || !OrgService.CanCompleteTask(org, role.Value))
                    return; // silently block
            }
            else
            {
                return; // personal task — only author or assignee
            }
        }

        task.IsCompleted = !task.IsCompleted;
        await _context.SaveChangesAsync();

        // Notifica os outros responsáveis quando a task for concluída
        if (task.IsCompleted && task.AssignedToUserIds != null)
        {
            var others = task.AssignedToUserIds
                .Where(uid => uid != completedByUserId)
                .ToList();

            foreach (var uid in others)
            {
                await _notificationService.CreateAsync(
                    userId: uid,
                    message: $"A tarefa \"{task.Title}\" foi marcada como concluída",
                    type: NotificationType.TaskCompleted,
                    link: $"/task/{task.Id}"
                );
            }
        }
    }

    public async Task<bool> HasPendingDependenciesAsync(int taskId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task?.DependencyOnTaskIds == null || !task.DependencyOnTaskIds.Any())
            return false;

        return await _context.Tasks
            .AnyAsync(t => task.DependencyOnTaskIds.Contains(t.Id) && !t.IsCompleted);
    }

    public async Task<List<TaskItem>> GetDependenciesAsync(int taskId)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task?.DependencyOnTaskIds == null || !task.DependencyOnTaskIds.Any())
            return new List<TaskItem>();

        return await _context.Tasks
            .Where(t => task.DependencyOnTaskIds.Contains(t.Id))
            .ToListAsync();
    }


    public async Task<HashSet<int>> GetDependencyChainAsync(int taskId)
    {
        var allTasks = await _context.Tasks.AsNoTracking().ToListAsync();
        var forbidden = new HashSet<int>();
        BuildAncestorChain(taskId, allTasks, forbidden);
        return forbidden;
    }

    // Percorre quem depende de taskId (ascendentes) — evita ciclo
    private void BuildAncestorChain(int taskId, List<TaskItem> allTasks, HashSet<int> visited)
    {
        // Encontrar tarefas que têm taskId como dependência
        var dependents = allTasks
            .Where(t => t.DependencyOnTaskIds != null && t.DependencyOnTaskIds.Contains(taskId))
            .ToList();

        foreach (var dep in dependents)
        {
            if (visited.Add(dep.Id))
                BuildAncestorChain(dep.Id, allTasks, visited);
        }
    }

    public async Task<List<TaskItem>> GetFilteredTasksAsync(string? userId, int? orgId, TaskFilter filter)
    {
        IQueryable<TaskItem> query = _context.Tasks
            .Include(t => t.Comments)
            .AsNoTracking();

        if (orgId.HasValue)
            query = query.Where(t => t.OrganizationId == orgId.Value);
        else if (!string.IsNullOrEmpty(userId))
            query = query.Where(t => t.AuthorUserId == userId
                && (t.OrganizationId == null || t.OrganizationId == 0));

        if (!string.IsNullOrEmpty(filter.Category))
            query = query.Where(t => t.Category == filter.Category);

        if (filter.IsCompleted.HasValue)
            query = query.Where(t => t.IsCompleted == filter.IsCompleted.Value);

        if (filter.MaxPriority.HasValue)
            query = query.Where(t => t.Priority <= filter.MaxPriority.Value);

        if (!string.IsNullOrEmpty(filter.Tag))
            query = query.Where(t => t.Tags.Contains(filter.Tag));

        if (!string.IsNullOrEmpty(filter.DueDateRange))
        {
            var today = DateTime.UtcNow.Date;
            query = filter.DueDateRange switch
            {
                "today" => query.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == today),
                "week" => query.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date >= today && t.DueDate.Value.Date <= today.AddDays(7)),
                "overdue" => query.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date < today && !t.IsCompleted),
                _ => query
            };
        }

        query = filter.SortBy switch
        {
            "duedate" => query.OrderBy(t => t.DueDate == null).ThenBy(t => t.DueDate),
            "created" => query.OrderByDescending(t => t.CreatedAt),
            _ => query.OrderBy(t => t.Priority).ThenBy(t => t.CreatedAt)
        };

        return await query.ToListAsync();
    }

    public async Task<int> GetPendingTaskCountForUserInOrgAsync(string userId, int orgId)
    {
        var tasks = await _context.Tasks
            .Where(t => t.OrganizationId == orgId && !t.IsCompleted)
            .AsNoTracking()
            .ToListAsync();

        return tasks.Count(t =>
            (t.AssignedToUserIds != null && t.AssignedToUserIds.Contains(userId))
            ||
            (t.ReviewByUserId != null && t.ReviewByUserId.Contains(userId)
                && !(t.ReviewedByUserId != null && t.ReviewedByUserId.Contains(userId)))
        );
    }

}