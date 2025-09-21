using System;
using SportFieldsManager.Api.Domain.Services;

namespace SportFieldsManager.Api.Models;

/// <summary>
/// Response payload after cancelling a registration.
/// </summary>
public class CancellationResponse
{
    public Guid TimeSlotId { get; init; }
    public Guid UserId { get; init; }
    public Guid? PromotedUserId { get; init; }
    public int ConfirmedParticipants { get; init; }
    public int Capacity { get; init; }
    public CancellationStatus Status { get; init; }
}
