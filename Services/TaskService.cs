using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.DTOs;

namespace TaskManager.Services;

public class TaskService
{
    private readonly AppDbContext _context;
    public TaskService(AppDbContext context)
    {
        _context = context;
    }
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

    public async Task UpdateTaskAsync(TaskItem task)
    {
        var existing = await _context.Tasks.FindAsync(task.Id);
        if (existing == null) return;

        // Copiar valores para o objeto já rastreado
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
    }

    public async Task DeleteTaskAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task != null)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
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

    public async Task ToggleCompleteAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task != null)
        {
            task.IsCompleted = !task.IsCompleted;
            await _context.SaveChangesAsync();
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
