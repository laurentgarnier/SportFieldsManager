using System;
using SportFieldsManager.Api.Domain.Entities;

namespace SportFieldsManager.Api.Models;

/// <summary>
/// Represents a notification entry returned by the API.
/// </summary>
public class NotificationResponse
{
    public Guid Id { get; init; }
    public string Recipient { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public DateTimeOffset SentAt { get; init; }
    public Guid? TimeSlotId { get; init; }
    public Guid? FieldId { get; init; }

    public static NotificationResponse FromEntity(EmailNotification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            Recipient = notification.Recipient,
            Subject = notification.Subject,
            Body = notification.Body,
            SentAt = notification.SentAt,
            TimeSlotId = notification.TimeSlotId,
            FieldId = notification.FieldId
        };
    }
}
