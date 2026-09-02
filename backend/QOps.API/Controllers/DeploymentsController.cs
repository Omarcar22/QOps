using Microsoft.AspNetCore.Mvc;
using QOps.Application.Deployments;

namespace QOps.API.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/environments/{environmentId:guid}/deployments")]
public sealed class DeploymentsController(IDeploymentService deploymentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DeploymentResponse>>> GetAll(
        Guid projectId,
        Guid environmentId,
        CancellationToken cancellationToken)
    {
        return Ok(await deploymentService.GetAllAsync(projectId, environmentId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeploymentResponse>> GetById(
        Guid projectId,
        Guid environmentId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var deployment = await deploymentService.GetByIdAsync(projectId, environmentId, id, cancellationToken);
        return deployment is null ? NotFound() : Ok(deployment);
    }

    [HttpPost]
    public async Task<ActionResult<DeploymentResponse>> Create(
        Guid projectId,
        Guid environmentId,
        CreateDeploymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var deployment = await deploymentService.CreateAsync(projectId, environmentId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { projectId, environmentId, id = deployment.Id }, deployment);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DeploymentResponse>> Update(
        Guid projectId,
        Guid environmentId,
        Guid id,
        UpdateDeploymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var deployment = await deploymentService.UpdateAsync(projectId, environmentId, id, request, cancellationToken);
            return deployment is null ? NotFound() : Ok(deployment);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid projectId,
        Guid environmentId,
        Guid id,
        CancellationToken cancellationToken)
    {
        return await deploymentService.DeleteAsync(projectId, environmentId, id, cancellationToken)
            ? NoContent()
            : NotFound();
    }
}
