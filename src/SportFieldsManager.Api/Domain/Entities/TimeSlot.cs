using System;
using System.Collections.Generic;
using System.Linq;

namespace SportFieldsManager.Api.Domain.Entities;

/// <summary>
/// Represents a reservation slot for a sport field.
/// </summary>
public class TimeSlot
{
    /// <summary>
    /// Gets or sets the time slot identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the related field identifier.
    /// </summary>
    public Guid FieldId { get; set; }

    /// <summary>
    /// Gets or sets the scheduled start date and time.
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// Gets or sets the scheduled end date and time.
    /// </summary>
    public DateTimeOffset EndTime { get; set; }

    /// <summary>
    /// Gets or sets an optional override for the maximum participants for the time slot.
    /// </summary>
    public int? MaximumParticipantsOverride { get; set; }

    /// <summary>
    /// Gets or sets an optional override for the minimum participants for the time slot.
    /// </summary>
    public int? MinimumParticipantsOverride { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a reminder was already sent for this time slot.
    /// </summary>
    public bool ReminderSent { get; set; }

    /// <summary>
    /// Gets or sets the registrations associated with the time slot.
    /// </summary>
    public List<TimeSlotRegistration> Registrations { get; set; } = new();

    /// <summary>
    /// Creates a deep copy of the time slot including registrations.
    /// </summary>
    /// <returns>A cloned <see cref="TimeSlot"/> instance.</returns>
    public TimeSlot Clone()
    {
        return new TimeSlot
        {
            Id = Id,
            FieldId = FieldId,
            StartTime = StartTime,
            EndTime = EndTime,
            MaximumParticipantsOverride = MaximumParticipantsOverride,
            MinimumParticipantsOverride = MinimumParticipantsOverride,
            ReminderSent = ReminderSent,
            Registrations = Registrations.Select(r => r.Clone()).ToList()
        };
    }
}
