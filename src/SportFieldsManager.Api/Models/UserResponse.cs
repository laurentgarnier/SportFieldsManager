using System;
using System.Collections.Generic;
using SportFieldsManager.Api.Domain.Entities;

namespace SportFieldsManager.Api.Models;

/// <summary>
/// Response payload describing a user profile.
/// </summary>
public class UserResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public IReadOnlyCollection<string> InterestedSports { get; init; } = Array.Empty<string>();

    public static UserResponse FromEntity(UserProfile profile)
    {
        return new UserResponse
        {
            Id = profile.Id,
            Email = profile.Email,
            FullName = profile.FullName,
            InterestedSports = profile.InterestedSports
        };
    }
}
