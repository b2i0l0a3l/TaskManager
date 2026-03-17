using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Common;
using TaskManager.Domain.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _RefreshTokenRepo;
    private readonly ILogger<AuthService> _Logger;

    public AuthService(IRefreshTokenRepository RefreshTokenRepo,ILogger<AuthService> Logger,UserManager<User> userManager, ITokenService tokenService)
    {
        _RefreshTokenRepo = RefreshTokenRepo;
        _userManager = userManager;
        _tokenService = tokenService;
        _Logger = Logger;
    }

    public async Task<ApiResponse<RegistreResponse>> RegisterAsync(RegisterRequest request)
    {
        try
        { 
            User? res = await _userManager.FindByEmailAsync(request.Email);
            if (res != null)
            {
                return ApiResponse<RegistreResponse>.ErrorResponse("Email already exists.");
            }
            User user = new()
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                string ErrorMessage = string.Join(", ", result.Errors.Select(x => x.Description));
                return ApiResponse<RegistreResponse>.ErrorResponse(ErrorMessage);
            }
            var RoleResult = await _userManager.AddToRoleAsync(user, Roles.User);
            if (!RoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                string ErrorMessage = string.Join(", ", RoleResult.Errors.Select(x => x.Description));
                return ApiResponse<RegistreResponse>.ErrorResponse(ErrorMessage);
            }
            return ApiResponse<RegistreResponse>.SuccessResponse(new RegistreResponse { Email = request.Email }, "Register Successfully");
        }catch(Exception ex)
        {
            _Logger.LogError($"Register Error : {ex.Message}");
            return ApiResponse<RegistreResponse>.ErrorResponse("Internal Error Happend!");
        }

    }

    public async Task<ApiResponse<AuthRespone>> LoginAsync(LoginRequest request)
    {
        try
        {
            User? user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return ApiResponse<AuthRespone>.ErrorResponse("Invalid email or password");
            bool isVerified = await _userManager.CheckPasswordAsync(user, request.Password);
            if(!isVerified)
                return ApiResponse<AuthRespone>.ErrorResponse("Invalid email or password");

            var token = _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            RefreshToken refresh = new()
            {
                RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken),
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
                RefreshTokenRevokedAt = null
            };
            await _RefreshTokenRepo.AddAsync(refresh);
            return ApiResponse<AuthRespone>.SuccessResponse(new AuthRespone
            {
                AccessToken = token,
                RefreshToken = refreshToken
            }, "Login successful");
            
        }catch(Exception ex)
        {
            _Logger.LogError($"Login Error : {ex.Message}");
            return ApiResponse<AuthRespone>.ErrorResponse("Internal Error Happend!");
        }

    }
}
