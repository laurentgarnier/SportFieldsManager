using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SportFieldsManager.Api.Domain.Entities;

namespace SportFieldsManager.Api.Domain.Services;

/// <summary>
/// Default in-memory implementation of <see cref="IBookingService"/>.
/// </summary>
public class BookingService : IBookingService
{
    private readonly Dictionary<Guid, SportField> _fields = new();
    private readonly Dictionary<Guid, TimeSlot> _timeSlots = new();
    private readonly Dictionary<Guid, UserProfile> _users = new();
    private readonly object _syncRoot = new();
    private readonly ILogger<BookingService> _logger;

    public BookingService(ILogger<BookingService> logger)
    {
        _logger = logger;
    }

    public Task<SportField> CreateFieldAsync(string name, string sport, int capacity, int minimumParticipants, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateFieldLimits(capacity, minimumParticipants);

        var normalizedName = NormalizeRequiredString(name, nameof(name));
        var normalizedSport = NormalizeRequiredString(sport, nameof(sport));

        var field = new SportField
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Sport = normalizedSport,
            Capacity = capacity,
            MinimumParticipants = minimumParticipants
        };

        lock (_syncRoot)
        {
            _fields[field.Id] = field.Clone();
        }

        _logger.LogInformation("Field {FieldId} created for sport {Sport}", field.Id, field.Sport);
        return Task.FromResult(field.Clone());
    }

    public Task<bool> DeleteFieldAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        bool removed;
        lock (_syncRoot)
        {
            removed = _fields.Remove(id);
            if (removed)
            {
                var slotsToRemove = _timeSlots.Values.Where(slot => slot.FieldId == id).Select(slot => slot.Id).ToList();
                foreach (var slotId in slotsToRemove)
                {
                    _timeSlots.Remove(slotId);
                }
            }
        }

        if (removed)
        {
            _logger.LogInformation("Field {FieldId} deleted", id);
        }

        return Task.FromResult(removed);
    }

    public Task<SportField?> GetFieldAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            return Task.FromResult(_fields.TryGetValue(id, out var field) ? field.Clone() : null);
        }
    }

    public Task<IReadOnlyCollection<SportField>> GetFieldsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            var results = _fields.Values.Select(field => field.Clone()).ToList();
            return Task.FromResult<IReadOnlyCollection<SportField>>(results);
        }
    }

    public Task<TimeSlot?> CreateTimeSlotAsync(Guid fieldId, DateTimeOffset startTime, DateTimeOffset endTime, int? maximumParticipants, int? minimumParticipants, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateTimeSlotRange(startTime, endTime);

        lock (_syncRoot)
        {
            if (!_fields.TryGetValue(fieldId, out var field))
            {
                return Task.FromResult<TimeSlot?>(null);
            }

            ValidateTimeSlotLimits(field, maximumParticipants, minimumParticipants);

            var timeSlot = new TimeSlot
            {
                Id = Guid.NewGuid(),
                FieldId = fieldId,
                StartTime = startTime,
                EndTime = endTime,
                MaximumParticipantsOverride = maximumParticipants,
                MinimumParticipantsOverride = minimumParticipants,
                ReminderSent = false
            };

            _timeSlots[timeSlot.Id] = timeSlot;
            _logger.LogInformation("Time slot {TimeSlotId} created on field {FieldId}", timeSlot.Id, fieldId);
            return Task.FromResult<TimeSlot?>(timeSlot.Clone());
        }
    }

    public Task<bool> DeleteTimeSlotAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        bool removed;
        lock (_syncRoot)
        {
            removed = _timeSlots.Remove(id);
        }

        if (removed)
        {
            _logger.LogInformation("Time slot {TimeSlotId} deleted", id);
        }

        return Task.FromResult(removed);
    }

    public Task<TimeSlot?> GetTimeSlotAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            return Task.FromResult(_timeSlots.TryGetValue(id, out var slot) ? slot.Clone() : null);
        }
    }

    public Task<IReadOnlyCollection<TimeSlot>> GetTimeSlotsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            var results = _timeSlots.Values.Select(slot => slot.Clone()).ToList();
            return Task.FromResult<IReadOnlyCollection<TimeSlot>>(results);
        }
    }

    public Task<IReadOnlyCollection<TimeSlot>> GetTimeSlotsForFieldAsync(Guid fieldId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            var results = _timeSlots.Values
                .Where(slot => slot.FieldId == fieldId)
                .Select(slot => slot.Clone())
                .ToList();
            return Task.FromResult<IReadOnlyCollection<TimeSlot>>(results);
        }
    }

    public Task<TimeSlot?> UpdateTimeSlotAsync(Guid id, DateTimeOffset startTime, DateTimeOffset endTime, int? maximumParticipants, int? minimumParticipants, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateTimeSlotRange(startTime, endTime);

        lock (_syncRoot)
        {
            if (!_timeSlots.TryGetValue(id, out var slot))
            {
                return Task.FromResult<TimeSlot?>(null);
            }

            if (!_fields.TryGetValue(slot.FieldId, out var field))
            {
                return Task.FromResult<TimeSlot?>(null);
            }

            ValidateTimeSlotLimits(field, maximumParticipants, minimumParticipants);

            var resolvedMaximum = maximumParticipants ?? field.Capacity;
            var confirmedParticipants = slot.Registrations.Count(r => !r.IsWaitlisted);
            if (confirmedParticipants > resolvedMaximum)
            {
                throw new InvalidOperationException("Cannot set the maximum participants below the number of confirmed registrations.");
            }

            slot.StartTime = startTime;
            slot.EndTime = endTime;
            slot.MaximumParticipantsOverride = maximumParticipants;
            slot.MinimumParticipantsOverride = minimumParticipants;

            if (slot.ReminderSent)
            {
                var resolvedMinimum = minimumParticipants ?? field.MinimumParticipants;
                if (confirmedParticipants < resolvedMinimum)
                {
                    slot.ReminderSent = false;
                }
            }

            _logger.LogInformation("Time slot {TimeSlotId} updated", id);
            return Task.FromResult<TimeSlot?>(slot.Clone());
        }
    }

    public Task<SportField?> UpdateFieldAsync(Guid id, string name, string sport, int capacity, int minimumParticipants, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateFieldLimits(capacity, minimumParticipants);

        var normalizedName = NormalizeRequiredString(name, nameof(name));
        var normalizedSport = NormalizeRequiredString(sport, nameof(sport));

        lock (_syncRoot)
        {
            if (!_fields.TryGetValue(id, out var field))
            {
                return Task.FromResult<SportField?>(null);
            }

            foreach (var slot in _timeSlots.Values.Where(slot => slot.FieldId == id))
            {
                var slotMaximum = slot.MaximumParticipantsOverride ?? capacity;
                if (slotMaximum > capacity)
                {
                    slotMaximum = capacity;
                }

                var confirmedParticipants = slot.Registrations.Count(r => !r.IsWaitlisted);
                if (confirmedParticipants > slotMaximum)
                {
                    throw new InvalidOperationException($"Cannot reduce the capacity to {capacity} because time slot {slot.Id} already has {confirmedParticipants} confirmed players.");
                }

                var slotMinimum = slot.MinimumParticipantsOverride ?? minimumParticipants;
                if (slotMinimum > capacity)
                {
                    throw new InvalidOperationException($"Cannot set the minimum participants to {minimumParticipants} because time slot {slot.Id} would become invalid.");
                }
            }

            field.Name = normalizedName;
            field.Sport = normalizedSport;
            field.Capacity = capacity;
            field.MinimumParticipants = minimumParticipants;

            foreach (var slot in _timeSlots.Values.Where(slot => slot.FieldId == id))
            {
                if (slot.MaximumParticipantsOverride.HasValue && slot.MaximumParticipantsOverride.Value > capacity)
                {
                    slot.MaximumParticipantsOverride = capacity;
                }

                var resolvedMinimum = slot.MinimumParticipantsOverride ?? minimumParticipants;
                var resolvedMaximum = slot.MaximumParticipantsOverride ?? capacity;
                if (resolvedMinimum > resolvedMaximum)
                {
                    slot.MinimumParticipantsOverride = resolvedMaximum;
                }
            }

            _logger.LogInformation("Field {FieldId} updated", id);
            return Task.FromResult<SportField?>(field.Clone());
        }
    }

    public Task<UserProfile> CreateUserAsync(string email, string fullName, IEnumerable<string> interestedSports, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedEmail = NormalizeEmail(email);
        var normalizedName = NormalizeRequiredString(fullName, nameof(fullName));
        var normalizedSports = NormalizeSports(interestedSports);

        var user = new UserProfile
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            FullName = normalizedName,
            InterestedSports = normalizedSports
        };

        lock (_syncRoot)
        {
            if (_users.Values.Any(existing => existing.Id != user.Id && string.Equals(existing.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"A user with the email '{normalizedEmail}' already exists.");
            }

            _users[user.Id] = user;
        }

        _logger.LogInformation("User {UserId} created", user.Id);
        return Task.FromResult(user.Clone());
    }

    public Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        bool removed;
        lock (_syncRoot)
        {
            removed = _users.Remove(id);
            if (removed)
            {
                foreach (var slot in _timeSlots.Values)
                {
                    var registration = slot.Registrations.FirstOrDefault(r => r.UserId == id);
                    if (registration is null)
                    {
                        continue;
                    }

                    var wasWaitlisted = registration.IsWaitlisted;
                    slot.Registrations.Remove(registration);

                    if (!wasWaitlisted)
                    {
                        var promoted = slot.Registrations
                            .Where(r => r.IsWaitlisted)
                            .OrderBy(r => r.RegisteredAt)
                            .FirstOrDefault();
                        if (promoted is not null)
                        {
                            promoted.IsWaitlisted = false;
                        }
                    }
                }
            }
        }

        if (removed)
        {
            _logger.LogInformation("User {UserId} deleted", id);
        }

        return Task.FromResult(removed);
    }

    public Task<UserProfile?> GetUserAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            return Task.FromResult(_users.TryGetValue(id, out var user) ? user.Clone() : null);
        }
    }

    public Task<IReadOnlyCollection<UserProfile>> GetUsersAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            var results = _users.Values.Select(user => user.Clone()).ToList();
            return Task.FromResult<IReadOnlyCollection<UserProfile>>(results);
        }
    }

    public Task<IReadOnlyCollection<UserProfile>> GetUsersByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var requestedIds = new HashSet<Guid>(userIds);
        lock (_syncRoot)
        {
            var results = _users
                .Where(pair => requestedIds.Contains(pair.Key))
                .Select(pair => pair.Value.Clone())
                .ToList();
            return Task.FromResult<IReadOnlyCollection<UserProfile>>(results);
        }
    }

    public Task<UserProfile?> UpdateUserAsync(Guid id, string email, string fullName, IEnumerable<string> interestedSports, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedEmail = NormalizeEmail(email);
        var normalizedName = NormalizeRequiredString(fullName, nameof(fullName));
        var normalizedSports = NormalizeSports(interestedSports);

        lock (_syncRoot)
        {
            if (!_users.TryGetValue(id, out var user))
            {
                return Task.FromResult<UserProfile?>(null);
            }

            if (_users.Values.Any(existing => existing.Id != id && string.Equals(existing.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"A user with the email '{normalizedEmail}' already exists.");
            }

            user.Email = normalizedEmail;
            user.FullName = normalizedName;
            user.InterestedSports = normalizedSports;

            _logger.LogInformation("User {UserId} updated", id);
            return Task.FromResult<UserProfile?>(user.Clone());
        }
    }

    public Task<RegistrationResult> RegisterUserAsync(Guid timeSlotId, Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (!_timeSlots.TryGetValue(timeSlotId, out var slot))
            {
                return Task.FromResult(new RegistrationResult { Status = RegistrationStatus.TimeSlotNotFound });
            }

            if (!_users.TryGetValue(userId, out var user))
            {
                return Task.FromResult(new RegistrationResult { Status = RegistrationStatus.UserNotFound });
            }

            if (!_fields.TryGetValue(slot.FieldId, out var field))
            {
                return Task.FromResult(new RegistrationResult { Status = RegistrationStatus.FieldNotFound });
            }

            if (slot.Registrations.Any(r => r.UserId == userId))
            {
                return Task.FromResult(new RegistrationResult
                {
                    Status = RegistrationStatus.AlreadyRegistered,
                    TimeSlot = slot.Clone(),
                    Field = field.Clone()
                });
            }

            var registration = new TimeSlotRegistration
            {
                UserId = userId,
                RegisteredAt = DateTimeOffset.UtcNow,
                IsWaitlisted = false
            };

            var capacity = slot.MaximumParticipantsOverride ?? field.Capacity;
            var confirmedParticipants = slot.Registrations.Count(r => !r.IsWaitlisted);
            if (confirmedParticipants >= capacity)
            {
                registration.IsWaitlisted = true;
            }

            slot.Registrations.Add(registration);

            var status = registration.IsWaitlisted ? RegistrationStatus.Waitlisted : RegistrationStatus.Confirmed;
            _logger.LogInformation("User {UserId} registered on time slot {TimeSlotId} with status {Status}", userId, timeSlotId, status);

            return Task.FromResult(new RegistrationResult
            {
                Status = status,
                Registration = registration.Clone(),
                TimeSlot = slot.Clone(),
                Field = field.Clone()
            });
        }
    }

    public Task<CancellationResult> CancelRegistrationAsync(Guid timeSlotId, Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (!_timeSlots.TryGetValue(timeSlotId, out var slot))
            {
                return Task.FromResult(new CancellationResult { Status = CancellationStatus.TimeSlotNotFound });
            }

            if (!_users.ContainsKey(userId))
            {
                return Task.FromResult(new CancellationResult { Status = CancellationStatus.UserNotFound });
            }

            if (!_fields.TryGetValue(slot.FieldId, out var field))
            {
                return Task.FromResult(new CancellationResult { Status = CancellationStatus.FieldNotFound });
            }

            var registration = slot.Registrations.FirstOrDefault(r => r.UserId == userId);
            if (registration is null)
            {
                return Task.FromResult(new CancellationResult
                {
                    Status = CancellationStatus.RegistrationNotFound,
                    TimeSlot = slot.Clone(),
                    Field = field.Clone()
                });
            }

            var wasWaitlisted = registration.IsWaitlisted;
            slot.Registrations.Remove(registration);

            Guid? promotedUserId = null;
            if (!wasWaitlisted)
            {
                var promoted = slot.Registrations
                    .Where(r => r.IsWaitlisted)
                    .OrderBy(r => r.RegisteredAt)
                    .FirstOrDefault();
                if (promoted is not null)
                {
                    promoted.IsWaitlisted = false;
                    promotedUserId = promoted.UserId;
                }
            }

            _logger.LogInformation("User {UserId} cancelled registration on time slot {TimeSlotId}", userId, timeSlotId);

            return Task.FromResult(new CancellationResult
            {
                Status = CancellationStatus.Cancelled,
                PromotedUserId = promotedUserId,
                TimeSlot = slot.Clone(),
                Field = field.Clone()
            });
        }
    }

    public Task<IReadOnlyCollection<ReminderCandidate>> GetTimeSlotsRequiringReminderAsync(DateTimeOffset now, DateTimeOffset threshold, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            var candidates = new List<ReminderCandidate>();
            foreach (var slot in _timeSlots.Values)
            {
                if (slot.ReminderSent)
                {
                    continue;
                }

                if (slot.StartTime <= now || slot.StartTime > threshold)
                {
                    continue;
                }

                if (!_fields.TryGetValue(slot.FieldId, out var field))
                {
                    continue;
                }

                var minimum = slot.MinimumParticipantsOverride ?? field.MinimumParticipants;
                var confirmedParticipants = slot.Registrations.Count(r => !r.IsWaitlisted);
                if (confirmedParticipants >= minimum)
                {
                    continue;
                }

                candidates.Add(new ReminderCandidate(slot.Clone(), field.Clone()));
            }

            return Task.FromResult<IReadOnlyCollection<ReminderCandidate>>(candidates);
        }
    }

    public Task MarkReminderSentAsync(Guid timeSlotId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            if (_timeSlots.TryGetValue(timeSlotId, out var slot))
            {
                slot.ReminderSent = true;
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<UserProfile>> GetUsersInterestedInSportAsync(string sport, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedSport = NormalizeRequiredString(sport, nameof(sport));

        lock (_syncRoot)
        {
            var results = _users.Values
                .Where(user => user.InterestedSports.Contains(normalizedSport))
                .Select(user => user.Clone())
                .ToList();
            return Task.FromResult<IReadOnlyCollection<UserProfile>>(results);
        }
    }

    private static void ValidateFieldLimits(int capacity, int minimumParticipants)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be a positive value.");
        }

        if (minimumParticipants <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumParticipants), "Minimum participants must be a positive value.");
        }

        if (minimumParticipants > capacity)
        {
            throw new ArgumentException("Minimum participants cannot be greater than capacity.", nameof(minimumParticipants));
        }
    }

    private static void ValidateTimeSlotLimits(SportField field, int? maximumParticipants, int? minimumParticipants)
    {
        if (maximumParticipants.HasValue && maximumParticipants.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumParticipants), "Maximum participants must be positive.");
        }

        if (minimumParticipants.HasValue && minimumParticipants.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumParticipants), "Minimum participants must be positive.");
        }

        var resolvedMaximum = maximumParticipants ?? field.Capacity;
        if (resolvedMaximum > field.Capacity)
        {
            throw new ArgumentException("Maximum participants cannot exceed the field capacity.", nameof(maximumParticipants));
        }

        var resolvedMinimum = minimumParticipants ?? field.MinimumParticipants;
        if (resolvedMinimum > resolvedMaximum)
        {
            throw new ArgumentException("Minimum participants cannot exceed the maximum participants.", nameof(minimumParticipants));
        }
    }

    private static void ValidateTimeSlotRange(DateTimeOffset startTime, DateTimeOffset endTime)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("The end time must be greater than the start time.", nameof(endTime));
        }
    }

    private static string NormalizeRequiredString(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        try
        {
            var mailAddress = new MailAddress(email.Trim());
            return mailAddress.Address;
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("The provided email address is invalid.", nameof(email), ex);
        }
    }

    private static HashSet<string> NormalizeSports(IEnumerable<string> sports)
    {
        var normalized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (sports is null)
        {
            return normalized;
        }

        foreach (var sport in sports)
        {
            if (string.IsNullOrWhiteSpace(sport))
            {
                continue;
            }

            normalized.Add(sport.Trim());
        }

        return normalized;
    }
}
