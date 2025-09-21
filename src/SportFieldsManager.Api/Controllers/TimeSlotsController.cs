using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SportFieldsManager.Api.Domain.Entities;
using SportFieldsManager.Api.Domain.Services;
using SportFieldsManager.Api.Models;

namespace SportFieldsManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimeSlotsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<TimeSlotsController> _logger;

    public TimeSlotsController(IBookingService bookingService, ILogger<TimeSlotsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TimeSlotResponse>), 200)]
    public async Task<ActionResult<IEnumerable<TimeSlotResponse>>> GetAllAsync([FromQuery] Guid? fieldId, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<TimeSlot> timeSlots;
        if (fieldId.HasValue)
        {
            timeSlots = await _bookingService.GetTimeSlotsForFieldAsync(fieldId.Value, cancellationToken);
        }
        else
        {
            timeSlots = await _bookingService.GetTimeSlotsAsync(cancellationToken);
        }

        var fields = await _bookingService.GetFieldsAsync(cancellationToken);
        var fieldDictionary = fields.ToDictionary(f => f.Id, f => f);

        var responses = timeSlots
            .Where(slot => fieldDictionary.ContainsKey(slot.FieldId))
            .Select(slot => TimeSlotResponse.FromEntities(slot, fieldDictionary[slot.FieldId]))
            .OrderBy(slot => slot.StartTime)
            .ToList();

        return Ok(responses);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TimeSlotResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TimeSlotResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var timeSlot = await _bookingService.GetTimeSlotAsync(id, cancellationToken);
        if (timeSlot is null)
        {
            return NotFound();
        }

        var field = await _bookingService.GetFieldAsync(timeSlot.FieldId, cancellationToken);
        if (field is null)
        {
            return NotFound();
        }

        var userIds = timeSlot.Registrations.Select(r => r.UserId).ToList();
        var users = await _bookingService.GetUsersByIdsAsync(userIds, cancellationToken);
        var userDictionary = users.ToDictionary(user => user.Id);

        var participants = timeSlot.Registrations
            .Select(registration => new TimeSlotParticipantResponse
            {
                UserId = registration.UserId,
                Email = userDictionary.TryGetValue(registration.UserId, out var user) ? user.Email : string.Empty,
                FullName = userDictionary.TryGetValue(registration.UserId, out var userProfile) ? userProfile.FullName : string.Empty,
                IsWaitlisted = registration.IsWaitlisted,
                RegisteredAt = registration.RegisteredAt
            })
            .OrderBy(participant => participant.RegisteredAt)
            .ToList();

        return Ok(TimeSlotResponse.FromEntities(timeSlot, field, participants));
    }

    [HttpGet("~/api/fields/{fieldId:guid}/timeslots")]
    [ProducesResponseType(typeof(IEnumerable<TimeSlotResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<TimeSlotResponse>>> GetByFieldAsync(Guid fieldId, CancellationToken cancellationToken)
    {
        var field = await _bookingService.GetFieldAsync(fieldId, cancellationToken);
        if (field is null)
        {
            return NotFound();
        }

        var timeSlots = await _bookingService.GetTimeSlotsForFieldAsync(fieldId, cancellationToken);
        var responses = timeSlots
            .Select(slot => TimeSlotResponse.FromEntities(slot, field))
            .OrderBy(slot => slot.StartTime)
            .ToList();

        return Ok(responses);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TimeSlotResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TimeSlotResponse>> CreateAsync([FromBody] TimeSlotRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var timeSlot = await _bookingService.CreateTimeSlotAsync(request.FieldId, request.StartTime, request.EndTime, request.MaximumParticipants, request.MinimumParticipants, cancellationToken);
            if (timeSlot is null)
            {
                return NotFound(new { message = "Field not found." });
            }

            var field = await _bookingService.GetFieldAsync(timeSlot.FieldId, cancellationToken);
            if (field is null)
            {
                return NotFound(new { message = "Field not found." });
            }

            return CreatedAtAction(nameof(GetByIdAsync), new { id = timeSlot.Id }, TimeSlotResponse.FromEntities(timeSlot, field));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return ValidationProblem(ModelState);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TimeSlotResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TimeSlotResponse>> UpdateAsync(Guid id, [FromBody] TimeSlotRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var current = await _bookingService.GetTimeSlotAsync(id, cancellationToken);
            if (current is null)
            {
                return NotFound();
            }

            if (current.FieldId != request.FieldId)
            {
                return BadRequest(new { message = "Changing the field of a time slot is not supported." });
            }

            var timeSlot = await _bookingService.UpdateTimeSlotAsync(id, request.StartTime, request.EndTime, request.MaximumParticipants, request.MinimumParticipants, cancellationToken);
            if (timeSlot is null)
            {
                return NotFound();
            }

            var field = await _bookingService.GetFieldAsync(timeSlot.FieldId, cancellationToken);
            if (field is null)
            {
                return NotFound();
            }

            return Ok(TimeSlotResponse.FromEntities(timeSlot, field));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return ValidationProblem(ModelState);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update time slot {TimeSlotId}", id);
            ModelState.AddModelError(string.Empty, ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _bookingService.DeleteTimeSlotAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/registrations")]
    [ProducesResponseType(typeof(RegistrationResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<RegistrationResponse>> RegisterAsync(Guid id, [FromBody] RegistrationRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _bookingService.RegisterUserAsync(id, request.UserId, cancellationToken);
            return result.Status switch
            {
                RegistrationStatus.Confirmed => Ok(CreateRegistrationResponse(result)),
                RegistrationStatus.Waitlisted => Ok(CreateRegistrationResponse(result)),
                RegistrationStatus.AlreadyRegistered => Conflict(new { message = "User already registered on this time slot." }),
                RegistrationStatus.TimeSlotNotFound => NotFound(new { message = "Time slot not found." }),
                RegistrationStatus.UserNotFound => NotFound(new { message = "User not found." }),
                RegistrationStatus.FieldNotFound => StatusCode(500, new { message = "Associated field not found." }),
                _ => BadRequest()
            };
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpDelete("{id:guid}/registrations/{userId:guid}")]
    [ProducesResponseType(typeof(CancellationResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CancellationResponse>> CancelAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var result = await _bookingService.CancelRegistrationAsync(id, userId, cancellationToken);
        return result.Status switch
        {
            CancellationStatus.Cancelled => Ok(CreateCancellationResponse(result, userId)),
            CancellationStatus.TimeSlotNotFound => NotFound(new { message = "Time slot not found." }),
            CancellationStatus.UserNotFound => NotFound(new { message = "User not found." }),
            CancellationStatus.RegistrationNotFound => NotFound(new { message = "Registration not found." }),
            _ => BadRequest()
        };
    }

    private static RegistrationResponse CreateRegistrationResponse(RegistrationResult result)
    {
        if (result.TimeSlot is null || result.Field is null || result.Registration is null)
        {
            throw new InvalidOperationException("Registration result is incomplete.");
        }

        var capacity = result.TimeSlot.MaximumParticipantsOverride ?? result.Field.Capacity;
        var confirmed = result.TimeSlot.Registrations.Count(r => !r.IsWaitlisted);

        return new RegistrationResponse
        {
            TimeSlotId = result.TimeSlot.Id,
            UserId = result.Registration.UserId,
            IsWaitlisted = result.Registration.IsWaitlisted,
            RegisteredAt = result.Registration.RegisteredAt,
            ConfirmedParticipants = confirmed,
            Capacity = capacity,
            Status = result.Status
        };
    }

    private static CancellationResponse CreateCancellationResponse(CancellationResult result, Guid userId)
    {
        if (result.TimeSlot is null || result.Field is null)
        {
            throw new InvalidOperationException("Cancellation result is incomplete.");
        }

        var capacity = result.TimeSlot.MaximumParticipantsOverride ?? result.Field.Capacity;
        var confirmed = result.TimeSlot.Registrations.Count(r => !r.IsWaitlisted);

        return new CancellationResponse
        {
            TimeSlotId = result.TimeSlot.Id,
            UserId = userId,
            PromotedUserId = result.PromotedUserId,
            ConfirmedParticipants = confirmed,
            Capacity = capacity,
            Status = result.Status
        };
    }
}
