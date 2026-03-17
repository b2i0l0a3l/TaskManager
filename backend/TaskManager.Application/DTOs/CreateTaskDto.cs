using System.ComponentModel.DataAnnotations;

namespace TaskManager.Application.DTOs;

public class CreateTaskDto
{
    [Required]
    [MinLength(3)]
    [MaxLength(80)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Description { get; set; }
}
