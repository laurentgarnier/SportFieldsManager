using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SportFieldsManager.Api.Domain.Entities;

namespace SportFieldsManager.Api.Domain.Services;

/// <summary>
/// In-memory implementation of <see cref="IEmailNotificationService"/> used for the sample application.
/// </summary>
public class InMemoryEmailNotificationService : IEmailNotificationService
{
    private readonly List<EmailNotification> _notifications = new();
    private readonly object _syncRoot = new();
    private readonly ILogger<InMemoryEmailNotificationService> _logger;

    public InMemoryEmailNotificationService(ILogger<InMemoryEmailNotificationService> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyCollection<EmailNotification>> GetNotificationsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            return Task.FromResult<IReadOnlyCollection<EmailNotification>>(_notifications.Select(n => n.Clone()).ToList());
        }
    }

    public Task SendReminderAsync(TimeSlot timeSlot, SportField field, IReadOnlyCollection<UserProfile> recipients, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (recipients.Count == 0)
        {
            return Task.CompletedTask;
        }

        var subject = $"Rappel - Session {field.Sport} le {timeSlot.StartTime:dd/MM/yyyy HH:mm}";
        var bodyBuilder = new StringBuilder();
        bodyBuilder.AppendLine($"Bonjour,");
        bodyBuilder.AppendLine();
        bodyBuilder.AppendLine($"Il manque encore des joueurs pour la session de {field.Sport} prévue le {timeSlot.StartTime:dddd dd MMMM yyyy à HH:mm}.");
        bodyBuilder.AppendLine("Inscrivez-vous rapidement pour confirmer la session !");

        var body = bodyBuilder.ToString();

        lock (_syncRoot)
        {
            foreach (var recipient in recipients)
            {
                var notification = new EmailNotification
                {
                    Id = Guid.NewGuid(),
                    Recipient = recipient.Email,
                    Subject = subject,
                    Body = body,
                    SentAt = DateTimeOffset.UtcNow,
                    TimeSlotId = timeSlot.Id,
                    FieldId = field.Id
                };

                _notifications.Add(notification);
                _logger.LogInformation("Reminder email queued for {Email} regarding time slot {TimeSlotId}", recipient.Email, timeSlot.Id);
            }
        }

        return Task.CompletedTask;
    }
}
