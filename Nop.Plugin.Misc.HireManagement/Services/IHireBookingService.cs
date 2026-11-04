using Nop.Plugin.Misc.HireManagement.Domain;
using Nop.Plugin.Misc.HireManagement.Models;

namespace Nop.Plugin.Misc.HireManagement.Services;

public partial interface IHireBookingService
{
    Task<HireBookingRecord> GetByIdAsync(int id);

    Task<IList<HireBookingRecord>> GetByProductAndMonthAsync(int productId, int year, int month);

    Task<IList<HireAvailabilityDay>> GetMonthlyAvailabilityAsync(int productId, int year, int month);

    Task<IList<HireAvailabilityDay>> GetMonthlyAvailabilityForDurationAsync(int productId, int year, int month, int durationDays);

    Task<(bool IsValid, string Error)> ValidateAsync(HireBookingRecord bookingRecord, int? existingRecordId = null, bool includeConflictCheck = true);

    Task<string> GetConflictWarningAsync(HireBookingRecord bookingRecord, int? existingRecordId = null);

    Task<bool> IsSelectionAvailableAsync(int productId, DateTime startUtc, DateTime endUtc, int requestedQuantity = 1);

    bool RequiresSurchargeReview(DateTime startStoreTime, DateTime endStoreTime);

    Task InsertAsync(HireBookingRecord bookingRecord);

    Task UpdateAsync(HireBookingRecord bookingRecord);

    Task DeleteAsync(HireBookingRecord bookingRecord);

    Task<(bool Saved, string Error)> SaveValidatedAsync(HireBookingRecord bookingRecord, int? existingRecordId = null, bool includeConflictCheck = true);
}