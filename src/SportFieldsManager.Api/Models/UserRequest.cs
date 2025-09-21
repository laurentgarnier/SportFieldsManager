using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SportFieldsManager.Api.Models;

/// <summary>
/// Request payload to create or update a user profile.
/// </summary>
public class UserRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    public IList<string> InterestedSports { get; set; } = new List<string>();
}
