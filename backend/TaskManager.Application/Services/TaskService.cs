using System.ComponentModel.DataAnnotations;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;

    public TaskService(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<List<TaskDto>>> GetAllAsync(string? status = null, string? search = null)
    {
        var tasks = await _repository.GetAllAsync(status, search);
        if (tasks == null || tasks.Count == 0) ApiResponse<List<TaskDto>>.ErrorResponse("No Tasks Found");
        List<TaskDto> dtos = tasks!.Select(x => TaskDto.convertToDTo(x)).ToList();
        return ApiResponse<List<TaskDto>>.SuccessResponse(dtos, "Tasks retrieved successfully");
    }

    public async Task<ApiResponse<TaskDto>> GetByIdAsync(int id)
    {
        if (id < 1)
            return ApiResponse<TaskDto>.ErrorResponse("Invalid task ID");
        var task = await _repository.GetByIdAsync(id);
        if (task == null) return ApiResponse<TaskDto>.ErrorResponse("Task not found");
        
        return ApiResponse<TaskDto>.SuccessResponse(TaskDto.convertToDTo(task), "Task retrieved successfully");
    }

    public async Task<ApiResponse<TaskDto>> CreateAsync(CreateTaskDto dto)
    {
        if (dto == null)
            return ApiResponse<TaskDto>.ErrorResponse("Data is required");
        DateTime now = DateTime.UtcNow;

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            UserId = "";
            Status = "pending",
            CreatedAt = now,
            UpdatedAt = now
        };
        var created = await _repository.AddAsync(task);
        return ApiResponse<TaskDto>.SuccessResponse(TaskDto.convertToDTo(created), "Task created successfully");
    }

    public async Task<ApiResponse<TaskDto>> UpdateAsync(int id, UpdateTaskDto dto)
    {
        if (dto == null)
            return ApiResponse<TaskDto>.ErrorResponse("Data is required");


        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return ApiResponse<TaskDto>.ErrorResponse("Task not found");
        if (dto.Title is not null) existing.Title = dto.Title;
        if (dto.Description is not null) existing.Description = dto.Description;
        if (dto.Status is not null) existing.Status = dto.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        return ApiResponse<TaskDto>.SuccessResponse(TaskDto.convertToDTo(updated), "Task updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return ApiResponse<bool>.ErrorResponse("Task not found");

        await _repository.DeleteAsync(id);
        return ApiResponse<bool>.SuccessResponse(true, "Task deleted successfully");
    }

    public async Task<ApiResponse<bool>> ClearAllAsync(string UserId)
    {
        await _repository.ClearAllAsync(UserId);
        return ApiResponse<bool>.SuccessResponse(true, "All tasks cleared successfully");
    }
}
