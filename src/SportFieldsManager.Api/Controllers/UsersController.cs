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
public class UsersController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IBookingService bookingService, ILogger<UsersController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), 200)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await _bookingService.GetUsersAsync(cancellationToken);
        return Ok(users.Select(UserResponse.FromEntity));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _bookingService.GetUserAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(UserResponse.FromEntity(user));
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<UserResponse>> CreateAsync([FromBody] UserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var user = await _bookingService.CreateUserAsync(request.Email, request.FullName, request.InterestedSports, cancellationToken);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = user.Id }, UserResponse.FromEntity(user));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return ValidationProblem(ModelState);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create user");
            ModelState.AddModelError(string.Empty, ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserResponse>> UpdateAsync(Guid id, [FromBody] UserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var user = await _bookingService.UpdateUserAsync(id, request.Email, request.FullName, request.InterestedSports, cancellationToken);
            if (user is null)
            {
                return NotFound();
            }

            return Ok(UserResponse.FromEntity(user));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return ValidationProblem(ModelState);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update user {UserId}", id);
            ModelState.AddModelError(string.Empty, ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _bookingService.DeleteUserAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
