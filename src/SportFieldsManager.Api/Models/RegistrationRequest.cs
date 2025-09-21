using System;
using System.ComponentModel.DataAnnotations;

namespace SportFieldsManager.Api.Models;

/// <summary>
/// Request payload to register a user on a time slot.
/// </summary>
public class RegistrationRequest
{
    [Required]
    public Guid UserId { get; set; }
}
