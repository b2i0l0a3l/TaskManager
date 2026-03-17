using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface ITaskRepository
{
    Task<List<TaskItem>?> GetAllAsync(string UserId,string? status = null, string? search = null);
    Task<TaskItem?> GetByIdAsync(int id);
    Task<TaskItem?> AddAsync(TaskItem task);
    Task<TaskItem?> UpdateAsync(TaskItem task);
    Task DeleteAsync(int id);
    Task ClearAllAsync(string UserId);
}
