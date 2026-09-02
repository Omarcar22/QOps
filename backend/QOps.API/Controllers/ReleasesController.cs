using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QOps.Application.Releases;

namespace QOps.API.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/releases")]
public sealed class ReleasesController(IReleaseService releaseService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ReleaseResponse>>> GetAll(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        return Ok(await releaseService.GetAllAsync(projectId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReleaseResponse>> GetById(
        Guid projectId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var release = await releaseService.GetByIdAsync(projectId, id, cancellationToken);
        return release is null ? NotFound() : Ok(release);
    }

    [HttpPost]
    [Authorize(Policy = "CanWrite")]
    public async Task<ActionResult<ReleaseResponse>> Create(
        Guid projectId,
        CreateReleaseRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var release = await releaseService.CreateAsync(projectId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { projectId, id = release.Id }, release);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanWrite")]
    public async Task<ActionResult<ReleaseResponse>> Update(
        Guid projectId,
        Guid id,
        UpdateReleaseRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var release = await releaseService.UpdateAsync(projectId, id, request, cancellationToken);
            return release is null ? NotFound() : Ok(release);
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
        return await releaseService.DeleteAsync(projectId, id, cancellationToken)
            ? NoContent()
            : NotFound();
    }
}
