using System;

namespace SportFieldsManager.Api.Domain.Entities;

/// <summary>
/// Represents the registration of a user on a time slot.
/// </summary>
public class TimeSlotRegistration
{
    /// <summary>
    /// Gets or sets the identifier of the registered user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the date on which the registration happened.
    /// </summary>
    public DateTimeOffset RegisteredAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the registration is waitlisted.
    /// </summary>
    public bool IsWaitlisted { get; set; }

    /// <summary>
    /// Creates a copy of the current registration.
    /// </summary>
    /// <returns>A cloned <see cref="TimeSlotRegistration"/> instance.</returns>
    public TimeSlotRegistration Clone()
    {
        return new TimeSlotRegistration
        {
            UserId = UserId,
            RegisteredAt = RegisteredAt,
            IsWaitlisted = IsWaitlisted
        };
    }
}
