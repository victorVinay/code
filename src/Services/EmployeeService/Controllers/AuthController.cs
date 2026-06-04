using EmployeeService.Data;
using EmployeeService.Models;
using EmployeeService.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Shared.Common;
using Shared.DTOs;
using Shared.Models;
using Shared.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IEmployeeRepository _repo;
    private readonly ITokenService _tokenService;
    private readonly EmployeeDbContext _context;

    public AuthController(IEmployeeRepository repo, ITokenService tokenService, EmployeeDbContext context)
    {
        _repo = repo;
        _tokenService = tokenService;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var traceId = HttpContext.TraceIdentifier;

        var user = (await _repo.GetAllAsync())
            .FirstOrDefault(x => x.Email == dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(ApiResponseFactory.Fail<object>("Invalid credentials", 401, traceId: traceId));

        var claims = new TokenClaims
        {
            Sub = user.Id.ToString(),
            Email = user.Email,
            FirstName = user.FirstName,
            Role = user.Role
        };

        var accessToken = _tokenService.GenerateAccessToken(claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            EmployeeId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync();

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

        return Ok(ApiResponseFactory.Success(response, "Login successful", 200, traceId));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(TokenRefreshDto dto)
    {
        var traceId = HttpContext.TraceIdentifier;

        var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
        var userId = Guid.Parse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub));

        var stored = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == dto.RefreshToken && !x.IsRevoked);

        if (stored == null || stored.ExpiresAt < DateTime.UtcNow)
            return Unauthorized(ApiResponseFactory.Fail<object>("Invalid refresh token", 401, traceId: traceId));

        stored.IsRevoked = true;

        var user = await _repo.GetByIdAsync(userId);

        var claims = new TokenClaims
        {
            Sub = user.Id.ToString(),
            Email = user.Email,
            FirstName = user.FirstName,
            Role = user.Role
        };

        var newAccessToken = _tokenService.GenerateAccessToken(claims);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            EmployeeId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync();

        var response = new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };

        return Ok(ApiResponseFactory.Success(response, "Token refreshed", 200, traceId));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutDto dto)
    {
        var traceId = HttpContext.TraceIdentifier;

        var token = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == dto.RefreshToken);
        if (token != null) token.IsRevoked = true;

        await _context.SaveChangesAsync();

        return Ok(ApiResponseFactory.Success<object>(null, "Logged out", 200, traceId));
    }
}