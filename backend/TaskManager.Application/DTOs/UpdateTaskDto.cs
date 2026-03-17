using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs;

public class UpdateTaskDto
{
    [MinLength(3)]
    [MaxLength(80)]
    public string? Title { get; set; }

    [MaxLength(300)]
    public string? Description { get; set; }

    public string? Status { get; set; }
}
