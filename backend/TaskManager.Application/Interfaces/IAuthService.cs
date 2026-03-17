using TaskManager.Application.DTOs;
using TaskManager.Domain.DTOs;

namespace TaskManager.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<RegistreResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthRespone>> LoginAsync(LoginRequest request);
}
