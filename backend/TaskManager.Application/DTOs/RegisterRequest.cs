using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManager.Application.DTOs
{
    public record RegisterRequest
    {
        [Required(ErrorMessage = "Full Name is Required")]
        public string FullName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email is Required")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage ="Password is Required")]
        public string Password { get; set; } = string.Empty;
    }
}