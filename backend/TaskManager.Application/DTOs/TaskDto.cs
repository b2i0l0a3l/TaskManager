using TaskManager.Domain.Entities;

namespace TaskManager.Application.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "pending";
    public long CreatedAt { get; set; }
    public long UpdatedAt { get; set; }

    public static TaskDto convertToDTo(TaskItem task)
    {
         return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            CreatedAt = new DateTimeOffset(task.CreatedAt, TimeSpan.Zero).ToUnixTimeMilliseconds(),
            UpdatedAt = new DateTimeOffset(task.UpdatedAt, TimeSpan.Zero).ToUnixTimeMilliseconds()
        };
    }
}
