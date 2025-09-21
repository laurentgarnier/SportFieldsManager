using SportFieldsManager.Api.Domain.Entities;

namespace SportFieldsManager.Api.Domain.Services;

/// <summary>
/// Provides the result of a registration attempt.
/// </summary>
public class RegistrationResult
{
    /// <summary>
    /// Gets or sets the resulting status of the registration attempt.
    /// </summary>
    public RegistrationStatus Status { get; init; }

    /// <summary>
    /// Gets or sets the registration record when the operation succeeded.
    /// </summary>
    public TimeSlotRegistration? Registration { get; init; }

    /// <summary>
    /// Gets or sets the updated time slot when available.
    /// </summary>
    public TimeSlot? TimeSlot { get; init; }

    /// <summary>
    /// Gets or sets the related field when available.
    /// </summary>
    public SportField? Field { get; init; }
}
