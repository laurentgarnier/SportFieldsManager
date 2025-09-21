using System;
using System.Collections.Generic;
using SportFieldsManager.Api.Domain.Entities;

namespace SportFieldsManager.Api.Models;

/// <summary>
/// Represents a time slot in the API responses.
/// </summary>
public class TimeSlotResponse
{
    public Guid Id { get; init; }
    public Guid FieldId { get; init; }
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public int Capacity { get; init; }
    public int MinimumParticipants { get; init; }
    public int ConfirmedParticipants { get; init; }
    public int WaitlistedParticipants { get; init; }
    public bool ReminderSent { get; init; }
    public IReadOnlyCollection<TimeSlotParticipantResponse>? Participants { get; init; }

    public static TimeSlotResponse FromEntities(TimeSlot slot, SportField field, IReadOnlyCollection<TimeSlotParticipantResponse>? participants = null)
    {
        var capacity = slot.MaximumParticipantsOverride ?? field.Capacity;
        var minimum = slot.MinimumParticipantsOverride ?? field.MinimumParticipants;
        var confirmed = slot.Registrations.Count(r => !r.IsWaitlisted);
        var waitlisted = slot.Registrations.Count(r => r.IsWaitlisted);

        return new TimeSlotResponse
        {
            Id = slot.Id,
            FieldId = slot.FieldId,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime,
            Capacity = capacity,
            MinimumParticipants = minimum,
            ConfirmedParticipants = confirmed,
            WaitlistedParticipants = waitlisted,
            ReminderSent = slot.ReminderSent,
            Participants = participants
        };
    }
}
