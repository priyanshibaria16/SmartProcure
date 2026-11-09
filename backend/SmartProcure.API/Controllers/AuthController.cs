using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProcure.Application.DTOs;
using SmartProcure.Application.Interfaces;

namespace SmartProcure.API.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        if (!response.Success)
            return Unauthorized(response);

        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var response = await _authService.GetUserProfileAsync(CurrentUserId);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var response = await _authService.ChangePasswordAsync(CurrentUserId, dto);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("/api/users")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] int? roleId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _authService.GetUsersAsync(search, roleId, page, pageSize);
        return Ok(response);
    }

    [HttpPost("/api/users")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateDto dto)
    {
        var response = await _authService.CreateUserAsync(dto);
        if (!response.Success)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetCurrentUser), new { id = response.Data!.UserId }, response);
    }

    [HttpPut("/api/users/{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
    {
        var response = await _authService.UpdateUserAsync(id, dto);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPatch("/api/users/{id}/status")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        var response = await _authService.ToggleUserStatusAsync(id);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
