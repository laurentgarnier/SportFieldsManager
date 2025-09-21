using System;

namespace SportFieldsManager.Api.Models;

/// <summary>
/// Represents a participant for a time slot in the API response.
/// </summary>
public class TimeSlotParticipantResponse
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public bool IsWaitlisted { get; init; }
    public DateTimeOffset RegisteredAt { get; init; }
}
