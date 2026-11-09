using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SmartProcure.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected int CurrentUserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out int id) ? id : 1; // Fallback to 1 for dev/seed
        }
    }

    protected string CurrentUserRole
    {
        get
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
