using System;
using SportFieldsManager.Api.Domain.Services;

namespace SportFieldsManager.Api.Models;

/// <summary>
/// Response payload after a registration attempt.
/// </summary>
public class RegistrationResponse
{
    public Guid TimeSlotId { get; init; }
    public Guid UserId { get; init; }
    public bool IsWaitlisted { get; init; }
    public DateTimeOffset RegisteredAt { get; init; }
    public int ConfirmedParticipants { get; init; }
    public int Capacity { get; init; }
    public RegistrationStatus Status { get; init; }
}
