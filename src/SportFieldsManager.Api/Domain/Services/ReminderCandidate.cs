using SportFieldsManager.Api.Domain.Entities;

namespace SportFieldsManager.Api.Domain.Services;

/// <summary>
/// Represents a time slot that requires a reminder notification.
/// </summary>
/// <param name="TimeSlot">The time slot that needs a reminder.</param>
/// <param name="Field">The associated field.</param>
public record class ReminderCandidate(TimeSlot TimeSlot, SportField Field);
