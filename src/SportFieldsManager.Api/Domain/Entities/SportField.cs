using System;

namespace SportFieldsManager.Api.Domain.Entities;

/// <summary>
/// Represents a sport field that can host reservation time slots.
/// </summary>
public class SportField
{
    /// <summary>
    /// Gets or sets the unique identifier of the field.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the field.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sport associated with the field.
    /// </summary>
    public string Sport { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum number of players that can participate on this field.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of players required to confirm a session on this field.
    /// </summary>
    public int MinimumParticipants { get; set; }

    /// <summary>
    /// Creates a deep copy of the field instance.
    /// </summary>
    /// <returns>A copy of the current <see cref="SportField"/>.</returns>
    public SportField Clone()
    {
        return new SportField
        {
            Id = Id,
            Name = Name,
            Sport = Sport,
            Capacity = Capacity,
            MinimumParticipants = MinimumParticipants
        };
    }
}
