using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SportFieldsManager.Api.Domain.Entities;

namespace SportFieldsManager.Api.Domain.Services;

/// <summary>
/// Provides the core operations to manage fields, time slots and registrations.
/// </summary>
public interface IBookingService
{
    Task<IReadOnlyCollection<SportField>> GetFieldsAsync(CancellationToken cancellationToken);
    Task<SportField?> GetFieldAsync(Guid id, CancellationToken cancellationToken);
    Task<SportField> CreateFieldAsync(string name, string sport, int capacity, int minimumParticipants, CancellationToken cancellationToken);
    Task<SportField?> UpdateFieldAsync(Guid id, string name, string sport, int capacity, int minimumParticipants, CancellationToken cancellationToken);
    Task<bool> DeleteFieldAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TimeSlot>> GetTimeSlotsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TimeSlot>> GetTimeSlotsForFieldAsync(Guid fieldId, CancellationToken cancellationToken);
    Task<TimeSlot?> GetTimeSlotAsync(Guid id, CancellationToken cancellationToken);
    Task<TimeSlot?> CreateTimeSlotAsync(Guid fieldId, DateTimeOffset startTime, DateTimeOffset endTime, int? maximumParticipants, int? minimumParticipants, CancellationToken cancellationToken);
    Task<TimeSlot?> UpdateTimeSlotAsync(Guid id, DateTimeOffset startTime, DateTimeOffset endTime, int? maximumParticipants, int? minimumParticipants, CancellationToken cancellationToken);
    Task<bool> DeleteTimeSlotAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UserProfile>> GetUsersAsync(CancellationToken cancellationToken);
    Task<UserProfile?> GetUserAsync(Guid id, CancellationToken cancellationToken);
    Task<UserProfile> CreateUserAsync(string email, string fullName, IEnumerable<string> interestedSports, CancellationToken cancellationToken);
    Task<UserProfile?> UpdateUserAsync(Guid id, string email, string fullName, IEnumerable<string> interestedSports, CancellationToken cancellationToken);
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserProfile>> GetUsersByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken);

    Task<RegistrationResult> RegisterUserAsync(Guid timeSlotId, Guid userId, CancellationToken cancellationToken);
    Task<CancellationResult> CancelRegistrationAsync(Guid timeSlotId, Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ReminderCandidate>> GetTimeSlotsRequiringReminderAsync(DateTimeOffset now, DateTimeOffset threshold, CancellationToken cancellationToken);
    Task MarkReminderSentAsync(Guid timeSlotId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserProfile>> GetUsersInterestedInSportAsync(string sport, CancellationToken cancellationToken);
}
