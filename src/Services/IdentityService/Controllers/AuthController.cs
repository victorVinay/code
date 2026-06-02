using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentityService.Data;
using IdentityService.DTOs;
using IdentityService.Models;
using IdentityService.Repositories.Interfaces;
using Shared.Models;
using Shared.Services;

namespace IdentityService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IdentityDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IAuditLogRepository auditLogRepository,
            IdentityDbContext context,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _auditLogRepository = auditLogRepository;
            _context = context;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<TokenResponse>>> Register([FromBody] RegisterRequest request)
        {
            var response = new ApiResponse<TokenResponse>
            {
                StatusCode = 400,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            // Validation
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                response.Success = false;
                response.Message = "Email and password are required";
                response.Errors.Add(new FieldError
                {
                    Field = "Email/Password",
                    Message = "Email and password cannot be empty"
                });
                return BadRequest(response);
            }

            // Check if user exists
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                response.Success = false;
                response.Message = "Email already registered";
                response.Errors.Add(new FieldError
                {
                    Field = "Email",
                    Message = "This email is already in use"
                });
                return BadRequest(response);
            }

            // Create new user
            var user = new User
            {
                UUID = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = "user",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _context.SaveChangesAsync();

            // Generate tokens
            var tokenClaims = new TokenClaims
            {
                Sub = user.UUID.ToString(),
                Email = user.Email,
                FirstName = user.FirstName,
                Role = user.Role,
                Jti = Guid.NewGuid().ToString()
            };

            var accessToken = _tokenService.GenerateAccessToken(tokenClaims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                UUID = Guid.NewGuid(),
                UserId = user.UUID,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();

            // Log audit
            var auditLog = new AuditLog
            {
                UUID = Guid.NewGuid(),
                UserId = user.UUID,
                Action = "USER_REGISTERED",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            response.Success = true;
            response.Message = "User registered successfully";
            response.StatusCode = 201;
            response.Data = new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = 900 // 15 minutes in seconds
            };

            return CreatedAtAction(nameof(Register), response);
        }

        /// <summary>
        /// Login user
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<TokenResponse>>> Login([FromBody] LoginRequest request)
        {
            var response = new ApiResponse<TokenResponse>
            {
                StatusCode = 401,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            // Validation
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                response.Success = false;
                response.Message = "Email and password are required";
                return Unauthorized(response);
            }

            // Find user
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                response.Success = false;
                response.Message = "Invalid email or password";
                return Unauthorized(response);
            }

            if (!user.IsActive)
            {
                response.Success = false;
                response.Message = "User account is inactive";
                return Unauthorized(response);
            }

            // Generate tokens
            var tokenClaims = new TokenClaims
            {
                Sub = user.UUID.ToString(),
                Email = user.Email,
                FirstName = user.FirstName,
                Role = user.Role,
                Jti = Guid.NewGuid().ToString()
            };

            var accessToken = _tokenService.GenerateAccessToken(tokenClaims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                UUID = Guid.NewGuid(),
                UserId = user.UUID,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();

            // Log audit
            var auditLog = new AuditLog
            {
                UUID = Guid.NewGuid(),
                UserId = user.UUID,
                Action = "USER_LOGIN",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            response.Success = true;
            response.Message = "Login successful";
            response.StatusCode = 200;
            response.Data = new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = 900 // 15 minutes in seconds
            };

            return Ok(response);
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<TokenResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var response = new ApiResponse<TokenResponse>
            {
                StatusCode = 401,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                response.Success = false;
                response.Message = "Refresh token is required";
                return Unauthorized(response);
            }

            // Find refresh token
            var refreshTokenEntity = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

            if (refreshTokenEntity == null || refreshTokenEntity.IsRevoked || refreshTokenEntity.ExpiresAt < DateTime.UtcNow)
            {
                response.Success = false;
                response.Message = "Invalid or expired refresh token";
                return Unauthorized(response);
            }

            var user = refreshTokenEntity.User;

            if (!user.IsActive)
            {
                response.Success = false;
                response.Message = "User account is inactive";
                return Unauthorized(response);
            }

            // Generate new access token
            var tokenClaims = new TokenClaims
            {
                Sub = user.UUID.ToString(),
                Email = user.Email,
                FirstName = user.FirstName,
                Role = user.Role,
                Jti = Guid.NewGuid().ToString()
            };

            var newAccessToken = _tokenService.GenerateAccessToken(tokenClaims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Revoke old refresh token
            refreshTokenEntity.IsRevoked = true;
            _refreshTokenRepository.Update(refreshTokenEntity);

            // Save new refresh token
            var newRefreshTokenEntity = new RefreshToken
            {
                UUID = Guid.NewGuid(),
                UserId = user.UUID,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            response.Success = true;
            response.Message = "Token refreshed successfully";
            response.StatusCode = 200;
            response.Data = new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                TokenType = "Bearer",
                ExpiresIn = 900
            };

            return Ok(response);
        }

        /// <summary>
        /// Logout user (revoke refresh token)
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<object>>> Logout()
        {
            var response = new ApiResponse<object>
            {
                StatusCode = 200,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                response.Success = false;
                response.Message = "Invalid user information";
                response.StatusCode = 400;
                return BadRequest(response);
            }

            // Revoke all active refresh tokens for this user
            var activeTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
            }

            _refreshTokenRepository.UpdateRange(activeTokens);

            // Log audit
            var auditLog = new AuditLog
            {
                UUID = Guid.NewGuid(),
                UserId = userId,
                Action = "USER_LOGOUT",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            response.Success = true;
            response.Message = "Logout successful";
            response.Data = null;

            return Ok(response);
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserInfoResponse>>> GetCurrentUser()
        {
            var response = new ApiResponse<UserInfoResponse>
            {
                StatusCode = 200,
                TraceId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                response.Success = false;
                response.Message = "Invalid user information";
                response.StatusCode = 400;
                return BadRequest(response);
            }

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found";
                response.StatusCode = 404;
                return NotFound(response);
            }

            response.Success = true;
            response.Message = "User information retrieved successfully";
            response.Data = new UserInfoResponse
            {
                UUID = user.UUID.ToString(),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                IsActive = user.IsActive
            };

            return Ok(response);
        }
    }
}
