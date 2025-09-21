using System;
using SportFieldsManager.Api.Domain.Entities;

namespace SportFieldsManager.Api.Domain.Services;

/// <summary>
/// Represents the result of a cancellation request.
/// </summary>
public class CancellationResult
{
    /// <summary>
    /// Gets or sets the outcome of the cancellation.
    /// </summary>
    public CancellationStatus Status { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the user who has been promoted from the wait list, when applicable.
    /// </summary>
    public Guid? PromotedUserId { get; init; }

    /// <summary>
    /// Gets or sets the updated time slot when available.
    /// </summary>
    public TimeSlot? TimeSlot { get; init; }

    /// <summary>
    /// Gets or sets the related field when available.
    /// </summary>
    public SportField? Field { get; init; }
}
