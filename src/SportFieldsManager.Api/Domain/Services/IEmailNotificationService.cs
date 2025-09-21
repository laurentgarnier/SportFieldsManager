using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SportFieldsManager.Api.Domain.Entities;

namespace SportFieldsManager.Api.Domain.Services;

/// <summary>
/// Represents the component responsible for sending email notifications.
/// </summary>
public interface IEmailNotificationService
{
    Task SendReminderAsync(TimeSlot timeSlot, SportField field, IReadOnlyCollection<UserProfile> recipients, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<EmailNotification>> GetNotificationsAsync(CancellationToken cancellationToken);
}
