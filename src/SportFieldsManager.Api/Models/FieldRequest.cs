using System.ComponentModel.DataAnnotations;

namespace SportFieldsManager.Api.Models;

/// <summary>
/// Request payload to create or update a sport field.
/// </summary>
public class FieldRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Sport { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    [Range(1, int.MaxValue)]
    public int MinimumParticipants { get; set; }
}
