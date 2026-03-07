using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Services;

public class TaskService
{
    private readonly AppDbContext _context;
    public TaskService(AppDbContext context)
    {
        _context = context;
    }
    public async Task<List<TaskItem>> GetAllTasksAsync(string? userId = null)
    {
        IQueryable<TaskItem> query = _context.Tasks.Include(t => t.Comments);

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.AuthorUserId == userId);
        }

        return await query
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();
    }


    public async Task<TaskItem?> GetTaskByIdAsync(int id, string? userId = null)
    {
        var query = _context.Tasks.Include(t => t.Comments).Where(t => t.Id == id);
        return await _context.Tasks
    .Include(t => t.Comments)
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
        _context.Tasks.Update(task);
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


}
