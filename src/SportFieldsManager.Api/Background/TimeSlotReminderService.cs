using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SportFieldsManager.Api.Domain.Services;

namespace SportFieldsManager.Api.Background;

/// <summary>
/// Background service responsible for sending reminder notifications for time slots that do not meet the minimum participation.
/// </summary>
public class TimeSlotReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<TimeSlotReminderService> _logger;
    private readonly TimeSpan _checkInterval;

    public TimeSlotReminderService(IServiceScopeFactory serviceScopeFactory, ILogger<TimeSlotReminderService> logger)
        : this(serviceScopeFactory, logger, TimeSpan.FromHours(12))
    {
    }

    public TimeSlotReminderService(IServiceScopeFactory serviceScopeFactory, ILogger<TimeSlotReminderService> logger, TimeSpan checkInterval)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _checkInterval = checkInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Time slot reminder service started with interval {Interval}", _checkInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown requested.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing reminders");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Time slot reminder service stopped");
    }

    private async Task ProcessRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();

        var now = DateTimeOffset.UtcNow;
        var threshold = now.AddDays(3);

        var candidates = await bookingService.GetTimeSlotsRequiringReminderAsync(now, threshold, cancellationToken);
        if (candidates.Count == 0)
        {
            _logger.LogDebug("No reminder needed at {Timestamp}", now);
            return;
        }

        foreach (var candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var participantIds = new HashSet<Guid>(candidate.TimeSlot.Registrations.Select(r => r.UserId));
            var interestedUsers = await bookingService.GetUsersInterestedInSportAsync(candidate.Field.Sport, cancellationToken);
            var recipients = interestedUsers
                .Where(user => !participantIds.Contains(user.Id))
                .ToList();

            if (recipients.Count == 0)
            {
                _logger.LogDebug("No recipients available for reminder of time slot {TimeSlotId}", candidate.TimeSlot.Id);
                continue;
            }

            await emailService.SendReminderAsync(candidate.TimeSlot, candidate.Field, recipients, cancellationToken);
            await bookingService.MarkReminderSentAsync(candidate.TimeSlot.Id, cancellationToken);
            _logger.LogInformation("Reminder sent for time slot {TimeSlotId}", candidate.TimeSlot.Id);
        }
    }
}
