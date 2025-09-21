using System;
using System.Collections.Generic;

namespace SportFieldsManager.Api.Domain.Entities;

/// <summary>
/// Represents a registered user of the booking system.
/// </summary>
public class UserProfile
{
    /// <summary>
    /// Gets or sets the identifier of the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full name of the user.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sports the user is interested in.
    /// </summary>
    public HashSet<string> InterestedSports { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Creates a deep copy of the user profile.
    /// </summary>
    /// <returns>A cloned instance of the current profile.</returns>
    public UserProfile Clone()
    {
        return new UserProfile
        {
            Id = Id,
            Email = Email,
            FullName = FullName,
            InterestedSports = new HashSet<string>(InterestedSports, StringComparer.OrdinalIgnoreCase)
        };
    }
}
