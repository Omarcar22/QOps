using Microsoft.AspNetCore.Mvc;
using QOps.Application.Users;

namespace QOps.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await authService.RegisterAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        return response is null ? Unauthorized() : Ok(response);
    }
}
