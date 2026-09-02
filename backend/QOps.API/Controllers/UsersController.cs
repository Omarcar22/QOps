using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QOps.Application.Users;

namespace QOps.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/users")]
public sealed class UsersController(IUserAdminService userAdminService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await userAdminService.GetAllAsync(cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserResponse>> Update(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userAdminService.UpdateAsync(id, request, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}
