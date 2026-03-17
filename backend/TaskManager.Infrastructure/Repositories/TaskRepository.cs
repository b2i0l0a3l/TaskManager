using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<TaskRepository> _Logger;
    public TaskRepository(AppDbContext context,ILogger<TaskRepository> Logger)
    {
        _context = context;
        _Logger = Logger;
    }

    public async Task<List<TaskItem>?> GetAllAsync(string UserId,string? status = null, string? search = null)
    {
        try
        {
            var query = _context.Tasks.Where(x=>x.UserId == UserId).AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status == status);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.ToLower();
                query = query.Where(t =>
                    t.Title.ToLower().Contains(q) ||
                    (t.Description != null && t.Description.ToLower().Contains(q)));
            }
            var result = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            return result;
        }
        catch(Exception ex)
        {
            _Logger.LogError($"Get All Async Error : {ex.Message}");
            return null;
        }
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(x=> x.Id == id);
    
        } catch(Exception ex)
        {
            _Logger.LogError($"Get By Id Error : {ex.Message}");
            return null;
        }
    }

    public async Task<TaskItem?> AddAsync(TaskItem task)
    {
        try
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task; 
        }catch(Exception ex)
        {
            _Logger.LogError($"Add Task Error : {ex.Message}");
            return null;
        }
    }

    public async Task<TaskItem?> UpdateAsync(TaskItem task)
    {
        try
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            return task;   
        }catch(Exception ex)
        {
            _Logger.LogError($"Update Task Error : {ex.Message}");
            return null;
        }
    }

    public async Task DeleteAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task != null)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearAllAsync(string UserId)
    {
        await _context.Tasks
        .Where(x => x.UserId == UserId)
        .ExecuteDeleteAsync();
    }
}
