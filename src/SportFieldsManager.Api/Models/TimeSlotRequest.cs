using System;
using System.ComponentModel.DataAnnotations;

namespace SportFieldsManager.Api.Models;

/// <summary>
/// Request payload for creating or updating a time slot.
/// </summary>
public class TimeSlotRequest
{
    [Required]
    public Guid FieldId { get; set; }

    [Required]
    public DateTimeOffset StartTime { get; set; }

    [Required]
    public DateTimeOffset EndTime { get; set; }

    [Range(1, int.MaxValue)]
    public int? MaximumParticipants { get; set; }

    [Range(1, int.MaxValue)]
    public int? MinimumParticipants { get; set; }
}
