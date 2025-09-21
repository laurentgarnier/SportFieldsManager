using System;
using SportFieldsManager.Api.Domain.Entities;

namespace SportFieldsManager.Api.Models;

/// <summary>
/// Response payload describing a sport field.
/// </summary>
public class FieldResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sport { get; init; } = string.Empty;
    public int Capacity { get; init; }
    public int MinimumParticipants { get; init; }

    public static FieldResponse FromEntity(SportField field)
    {
        return new FieldResponse
        {
            Id = field.Id,
            Name = field.Name,
            Sport = field.Sport,
            Capacity = field.Capacity,
            MinimumParticipants = field.MinimumParticipants
        };
    }
}
