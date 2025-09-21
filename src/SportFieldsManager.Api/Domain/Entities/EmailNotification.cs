using System;

namespace SportFieldsManager.Api.Domain.Entities;

/// <summary>
/// Represents an email notification sent by the system.
/// </summary>
public class EmailNotification
{
    /// <summary>
    /// Gets or sets the unique identifier of the notification.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the email recipient.
    /// </summary>
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subject of the notification.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the notification body.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time the notification was sent.
    /// </summary>
    public DateTimeOffset SentAt { get; set; }

    /// <summary>
    /// Gets or sets the related time slot identifier.
    /// </summary>
    public Guid? TimeSlotId { get; set; }

    /// <summary>
    /// Gets or sets the related field identifier.
    /// </summary>
    public Guid? FieldId { get; set; }

    /// <summary>
    /// Creates a copy of the current notification.
    /// </summary>
    /// <returns>A cloned instance of <see cref="EmailNotification"/>.</returns>
    public EmailNotification Clone()
    {
        return new EmailNotification
        {
            Id = Id,
            Recipient = Recipient,
            Subject = Subject,
            Body = Body,
            SentAt = SentAt,
            TimeSlotId = TimeSlotId,
            FieldId = FieldId
        };
    }
}
