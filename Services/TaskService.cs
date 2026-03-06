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
    public async Task<List<TaskItem>> GetAllTasksAsync()
    {
        return await _context.Tasks
        .Include(t => t.Comments)
        .OrderByDescending(t => t.Priority)
        .ThenBy(t => t.CreatedAt)
        .ToListAsync();
    }

    public async Task<TaskItem?> GetTaskByIdAsync(int id)
    {
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

}
