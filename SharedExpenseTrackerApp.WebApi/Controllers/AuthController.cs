using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedExpenseTrackerApp.Domain.Features.Auth;
using SharedExpenseTrackerApp.Domain.Models.Auth;
using SharedExpenseTrackerApp.Domain.Shared;
using System.Security.Claims;

namespace SharedExpenseTrackerApp.WebApi.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterReqModel request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        return Execute(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginReqModel request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        return Execute(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!long.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _authService.GetCurrentUserAsync(userId, ct);
        return Execute(result);
    }
}
