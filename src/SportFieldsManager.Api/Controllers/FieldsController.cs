using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportFieldsManager.Api.Domain.Services;
using SportFieldsManager.Api.Models;

namespace SportFieldsManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FieldsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<FieldsController> _logger;

    public FieldsController(IBookingService bookingService, ILogger<FieldsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FieldResponse>), 200)]
    public async Task<ActionResult<IEnumerable<FieldResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var fields = await _bookingService.GetFieldsAsync(cancellationToken);
        return Ok(fields.Select(FieldResponse.FromEntity));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FieldResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<FieldResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var field = await _bookingService.GetFieldAsync(id, cancellationToken);
        if (field is null)
        {
            return NotFound();
        }

        return Ok(FieldResponse.FromEntity(field));
    }

    [HttpPost]
    [ProducesResponseType(typeof(FieldResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<FieldResponse>> CreateAsync([FromBody] FieldRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var field = await _bookingService.CreateFieldAsync(request.Name, request.Sport, request.Capacity, request.MinimumParticipants, cancellationToken);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = field.Id }, FieldResponse.FromEntity(field));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FieldResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<FieldResponse>> UpdateAsync(Guid id, [FromBody] FieldRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var field = await _bookingService.UpdateFieldAsync(id, request.Name, request.Sport, request.Capacity, request.MinimumParticipants, cancellationToken);
            if (field is null)
            {
                return NotFound();
            }

            return Ok(FieldResponse.FromEntity(field));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return ValidationProblem(ModelState);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update field {FieldId}", id);
            ModelState.AddModelError(string.Empty, ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _bookingService.DeleteFieldAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
