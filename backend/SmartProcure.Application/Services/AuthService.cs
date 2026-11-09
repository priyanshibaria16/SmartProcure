using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartProcure.Application.Common;
using SmartProcure.Application.Common.Interfaces;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Interfaces;
using SmartProcure.Domain.Entities;

namespace SmartProcure.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiResponse<LoginResponse>.Fail("Email and password are required");
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.Trim().ToLower());

        if (user == null || !user.IsActive)
        {
            return ApiResponse<LoginResponse>.Fail("Invalid credentials or account deactivated");
        }

        if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
        {
            return ApiResponse<LoginResponse>.Fail("Invalid credentials or account deactivated");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var (token, expiresAt) = GenerateJwtToken(user);

        return ApiResponse<LoginResponse>.Ok(new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.DepartmentName ?? string.Empty,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName ?? string.Empty,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt
            }
        }, "Login successful");
    }

    public async Task<ApiResponse<UserDto>> GetUserProfileAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null) return ApiResponse<UserDto>.Fail("User not found");

        return ApiResponse<UserDto>.Ok(new UserDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            DepartmentId = user.DepartmentId,
            DepartmentName = user.Department?.DepartmentName ?? string.Empty,
            RoleId = user.RoleId,
            RoleName = user.Role?.RoleName ?? string.Empty,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt
        });
    }

    public async Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(string? search, int? roleId, int page, int pageSize)
    {
        var query = _context.Users
            .Include(u => u.Role)
            .Include(u => u.Department)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(s) || u.Email.ToLower().Contains(s));
        }

        if (roleId.HasValue)
        {
            query = query.Where(u => u.RoleId == roleId.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(u => u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                Email = u.Email,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                DepartmentId = u.DepartmentId,
                DepartmentName = u.Department != null ? u.Department.DepartmentName : string.Empty,
                RoleId = u.RoleId,
                RoleName = u.Role != null ? u.Role.RoleName : string.Empty,
                IsActive = u.IsActive,
                LastLoginAt = u.LastLoginAt
            })
            .ToListAsync();

        return ApiResponse<PagedResult<UserDto>>.Ok(new PagedResult<UserDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(UserCreateDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.Trim().ToLower()))
        {
            return ApiResponse<UserDto>.Fail("A user with this email already exists");
        }

        var user = new User
        {
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = PasswordHelper.HashPassword(dto.Password),
            FullName = dto.FullName.Trim(),
            PhoneNumber = dto.PhoneNumber.Trim(),
            DepartmentId = dto.DepartmentId,
            RoleId = dto.RoleId,
            IsActive = true
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return await GetUserProfileAsync(user.UserId);
    }

    public async Task<ApiResponse<UserDto>> UpdateUserAsync(int id, UserUpdateDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null) return ApiResponse<UserDto>.Fail("User not found");

        user.FullName = dto.FullName.Trim();
        user.PhoneNumber = dto.PhoneNumber.Trim();
        user.DepartmentId = dto.DepartmentId;
        user.RoleId = dto.RoleId;
        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetUserProfileAsync(id);
    }

    public async Task<ApiResponse<bool>> ToggleUserStatusAsync(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null) return ApiResponse<bool>.Fail("User not found");

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(user.IsActive, $"User account is now {(user.IsActive ? "Active" : "Inactive")}");
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null) return ApiResponse<bool>.Fail("User not found");

        if (!PasswordHelper.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
        {
            return ApiResponse<bool>.Fail("Current password is incorrect");
        }

        user.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Password changed successfully");
    }

    private (string token, DateTime expiresAt) GenerateJwtToken(User user)
    {
        var jwtSecret = _configuration["Jwt:SecretKey"] ?? "SmartProcure_Enterprise_Super_Secret_Key_2026_Secure_JWT_Token_Generator_Key!";
        var issuer = _configuration["Jwt:Issuer"] ?? "SmartProcure.API";
        var audience = _configuration["Jwt:Audience"] ?? "SmartProcure.Client";
        var expiresAt = DateTime.UtcNow.AddHours(8); // 8-hour shift

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role?.RoleName ?? "Viewer"),
            new("DepartmentId", user.DepartmentId.ToString()),
            new("DepartmentName", user.Department?.DepartmentName ?? string.Empty)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return (tokenHandler.WriteToken(tokenDescriptor), expiresAt);
    }
}
