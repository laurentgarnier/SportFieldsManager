namespace SportFieldsManager.Api.Domain.Services;

/// <summary>
/// Possible outcomes for a cancellation attempt.
/// </summary>
public enum CancellationStatus
{
    /// <summary>
    /// The registration has been cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    /// The time slot does not exist.
    /// </summary>
    TimeSlotNotFound,

    /// <summary>
    /// The participant does not exist.
    /// </summary>
    UserNotFound,

    /// <summary>
    /// The participant was not registered on the specified slot.
    /// </summary>
    RegistrationNotFound
}
