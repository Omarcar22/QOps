using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QOps.Application.Environments;

namespace QOps.API.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/environments")]
public sealed class EnvironmentsController(IEnvironmentService environmentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<EnvironmentResponse>>> GetAll(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        return Ok(await environmentService.GetAllAsync(projectId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EnvironmentResponse>> GetById(
        Guid projectId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var environment = await environmentService.GetByIdAsync(projectId, id, cancellationToken);
        return environment is null ? NotFound() : Ok(environment);
    }

    [HttpPost]
    [Authorize(Policy = "CanWrite")]
    public async Task<ActionResult<EnvironmentResponse>> Create(
        Guid projectId,
        CreateEnvironmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var environment = await environmentService.CreateAsync(projectId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { projectId, id = environment.Id }, environment);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanWrite")]
    public async Task<ActionResult<EnvironmentResponse>> Update(
        Guid projectId,
        Guid id,
        UpdateEnvironmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var environment = await environmentService.UpdateAsync(projectId, id, request, cancellationToken);
            return environment is null ? NotFound() : Ok(environment);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanDelete")]
    public async Task<IActionResult> Delete(Guid projectId, Guid id, CancellationToken cancellationToken)
    {
        return await environmentService.DeleteAsync(projectId, id, cancellationToken)
            ? NoContent()
            : NotFound();
    }
}
