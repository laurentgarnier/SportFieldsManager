namespace SportFieldsManager.Api.Domain.Services;

/// <summary>
/// Possible outcomes for a registration attempt.
/// </summary>
public enum RegistrationStatus
{
    /// <summary>
    /// The registration succeeded and the participant has a confirmed seat.
    /// </summary>
    Confirmed,

    /// <summary>
    /// The registration succeeded but the participant has been put on a wait list.
    /// </summary>
    Waitlisted,

    /// <summary>
    /// The participant was already registered.
    /// </summary>
    AlreadyRegistered,

    /// <summary>
    /// The target time slot does not exist.
    /// </summary>
    TimeSlotNotFound,

    /// <summary>
    /// The provided user does not exist.
    /// </summary>
    UserNotFound,

    /// <summary>
    /// The time slot references an unknown field.
    /// </summary>
    FieldNotFound
}
