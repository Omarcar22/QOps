using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QOps.Application.Pipelines;

namespace QOps.API.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/pipelines")]
public sealed class PipelinesController(IPipelineService pipelineService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PipelineResponse>>> GetAll(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        return Ok(await pipelineService.GetAllAsync(projectId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PipelineResponse>> GetById(
        Guid projectId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var pipeline = await pipelineService.GetByIdAsync(projectId, id, cancellationToken);
        return pipeline is null ? NotFound() : Ok(pipeline);
    }

    [HttpPost]
    [Authorize(Policy = "CanWrite")]
    public async Task<ActionResult<PipelineResponse>> Create(
        Guid projectId,
        CreatePipelineRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var pipeline = await pipelineService.CreateAsync(projectId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { projectId, id = pipeline.Id }, pipeline);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanWrite")]
    public async Task<ActionResult<PipelineResponse>> Update(
        Guid projectId,
        Guid id,
        UpdatePipelineRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var pipeline = await pipelineService.UpdateAsync(projectId, id, request, cancellationToken);
            return pipeline is null ? NotFound() : Ok(pipeline);
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
        return await pipelineService.DeleteAsync(projectId, id, cancellationToken)
            ? NoContent()
            : NotFound();
    }

    [HttpGet("{pipelineId:guid}/executions")]
    public async Task<ActionResult<IReadOnlyCollection<PipelineExecutionResponse>>> GetExecutions(
        Guid projectId,
        Guid pipelineId,
        CancellationToken cancellationToken)
    {
        var pipeline = await pipelineService.GetByIdAsync(projectId, pipelineId, cancellationToken);
        if (pipeline is null)
        {
            return NotFound();
        }

        return Ok(await pipelineService.GetExecutionsAsync(projectId, pipelineId, cancellationToken));
    }

    [HttpPost("{pipelineId:guid}/executions")]
    [Authorize(Policy = "CanWrite")]
    public async Task<ActionResult<PipelineExecutionResponse>> CreateExecution(
        Guid projectId,
        Guid pipelineId,
        CreatePipelineExecutionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var execution = await pipelineService.CreateExecutionAsync(projectId, pipelineId, request, cancellationToken);
            return CreatedAtAction(nameof(GetExecutions), new { projectId, pipelineId }, execution);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }
}
