using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<ApiResponse<List<TaskDto>>> GetAllAsync(string? status = null, string? search = null);
    Task<ApiResponse<TaskDto>> GetByIdAsync(int id);
    Task<ApiResponse<TaskDto>> CreateAsync(CreateTaskDto dto);
    Task<ApiResponse<TaskDto>> UpdateAsync(int id, UpdateTaskDto dto);
    Task<ApiResponse<bool>> DeleteAsync(int id);
    Task<ApiResponse<bool>> ClearAllAsync(string UserId);
}
