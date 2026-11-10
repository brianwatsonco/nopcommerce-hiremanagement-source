using Nop.Data;
using Nop.Plugin.Misc.HireManagement.Domain;
using Nop.Plugin.Misc.HireManagement.Models;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using System.Globalization;

namespace Nop.Plugin.Misc.HireManagement.Services;

public partial class HireBookingService : IHireBookingService
{
    private readonly IRepository<HireBookingRecord> _hireBookingRecordRepository;
    private readonly IProductService _productService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly HireManagementSettings _hireManagementSettings;

    public HireBookingService(IRepository<HireBookingRecord> hireBookingRecordRepository,
        IProductService productService,
        IDateTimeHelper dateTimeHelper,
        HireManagementSettings hireManagementSettings)
    {
        _hireBookingRecordRepository = hireBookingRecordRepository;
        _productService = productService;
        _dateTimeHelper = dateTimeHelper;
        _hireManagementSettings = hireManagementSettings;
    }

    public async Task<HireBookingRecord> GetByIdAsync(int id)
    {
        return await _hireBookingRecordRepository.GetByIdAsync(id);
    }

    public async Task<IList<HireBookingRecord>> GetByProductAndMonthAsync(int productId, int year, int month)
    {
        var fromUtc = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = fromUtc.AddMonths(1).AddTicks(-1);

        return await _hireBookingRecordRepository.Table
            .Where(record => record.ProductId == productId && record.StartUtc <= toUtc && record.EndUtc >= fromUtc && !record.IsCancelled)
            .OrderBy(record => record.StartUtc)
            .ToListAsync();
    }

    public async Task<IList<HireAvailabilityDay>> GetMonthlyAvailabilityAsync(int productId, int year, int month)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.Deleted || !product.Published)
            return [];

        var totalStock = product.StockQuantity;
        var lowAvailabilityThreshold = Math.Max(product.MinStockQuantity, 0);

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var availability = new List<HireAvailabilityDay>(daysInMonth);

        var monthStartUtc = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEndUtc = monthStartUtc.AddMonths(1).AddTicks(-1);

        var records = await _hireBookingRecordRepository.Table
            .Where(record => record.ProductId == productId && !record.IsCancelled && record.StartUtc <= monthEndUtc && record.EndUtc >= monthStartUtc)
            .ToListAsync();

        for (var day = 1; day <= daysInMonth; day++)
        {
            var dayStartUtc = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
            var dayEndUtc = dayStartUtc.AddDays(1).AddTicks(-1);

            var dayState = EvaluateStateForWindow(records, dayStartUtc, dayEndUtc, totalStock, lowAvailabilityThreshold);
            var unavailableQuantity = CalculateUnavailableQuantityForWindow(records, dayStartUtc, dayEndUtc, totalStock);
            var bookedQuantity = CalculateBookedQuantityForWindow(records, dayStartUtc, dayEndUtc);
            var affectedQuantity = Math.Min(Math.Max(unavailableQuantity + bookedQuantity, 0), Math.Max(totalStock, 0));
            var remainingQuantity = Math.Max(Math.Max(totalStock, 0) - affectedQuantity, 0);

            availability.Add(new HireAvailabilityDay
            {
                Date = dayStartUtc.Date,
                State = dayState,
                TotalQuantity = Math.Max(totalStock, 0),
                AffectedQuantity = affectedQuantity,
                RemainingQuantity = remainingQuantity
            });
        }

        return availability;
    }

    public async Task<IList<HireAvailabilityDay>> GetMonthlyAvailabilityForDurationAsync(int productId, int year, int month, int durationDays)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.Deleted || !product.Published)
            return [];

        var totalStock = product.StockQuantity;
        var lowAvailabilityThreshold = Math.Max(product.MinStockQuantity, 0);

        if (durationDays < 1)
            durationDays = 1;

        var todayStoreDate = _dateTimeHelper
            .ConvertToUserTime(DateTime.UtcNow, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone)
            .Date;

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var availability = new List<HireAvailabilityDay>(daysInMonth);

        var monthStartUtc = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEndUtc = monthStartUtc.AddMonths(1).AddTicks(-1);

        var searchFromUtc = monthStartUtc.AddHours(-_hireManagementSettings.BufferHoursBefore - _hireManagementSettings.BufferHoursAfter - (durationDays * 24d));
        var searchToUtc = monthEndUtc.AddHours(_hireManagementSettings.BufferHoursBefore + _hireManagementSettings.BufferHoursAfter + (durationDays * 24d));

        var records = await _hireBookingRecordRepository.Table
            .Where(record => record.ProductId == productId && !record.IsCancelled && record.StartUtc <= searchToUtc && record.EndUtc >= searchFromUtc)
            .ToListAsync();

        for (var day = 1; day <= daysInMonth; day++)
        {
            var startStoreDate = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);

            if (startStoreDate.Date < todayStoreDate)
            {
                availability.Add(new HireAvailabilityDay
                {
                    Date = startStoreDate.Date,
                    State = HireAvailabilityState.Unavailable
                });

                continue;
            }

            var (candidateStartUtc, candidateEndUtc) = BuildDurationWindowUtc(startStoreDate, durationDays);

            var dayState = EvaluateStateForWindow(records, candidateStartUtc, candidateEndUtc, totalStock, lowAvailabilityThreshold);
            var unavailableQuantity = CalculateUnavailableQuantityForWindow(records, candidateStartUtc, candidateEndUtc, totalStock);
            var bookedQuantity = CalculateBookedQuantityForWindow(records, candidateStartUtc, candidateEndUtc);
            var affectedQuantity = Math.Min(Math.Max(unavailableQuantity + bookedQuantity, 0), Math.Max(totalStock, 0));
            var remainingQuantity = Math.Max(Math.Max(totalStock, 0) - affectedQuantity, 0);

            availability.Add(new HireAvailabilityDay
            {
                Date = startStoreDate.Date,
                State = dayState,
                TotalQuantity = Math.Max(totalStock, 0),
                AffectedQuantity = affectedQuantity,
                RemainingQuantity = remainingQuantity
            });
        }

        return availability;
    }

    public async Task<(bool IsValid, string Error)> ValidateAsync(HireBookingRecord bookingRecord, int? existingRecordId = null)
    {
        if (bookingRecord.ProductId <= 0)
            return (false, "Product is required.");

        if (bookingRecord.StartUtc >= bookingRecord.EndUtc)
            return (false, "End date and time must be later than start date and time.");

        if (bookingRecord.Status == HireAvailabilityState.Available)
            return (false, "Only FullyBooked or Unavailable records can be stored.");

        if (bookingRecord.Status == HireAvailabilityState.FullyBooked)
        {
            if (!bookingRecord.CustomerId.HasValue || bookingRecord.CustomerId.Value <= 0)
                return (false, "Customer ID is required when status is FullyBooked.");

            if (!bookingRecord.OrderId.HasValue || bookingRecord.OrderId.Value <= 0)
                return (false, "Order ID is required when status is FullyBooked.");
        }

        var newStartUtc = GetEffectiveBlockStart(bookingRecord);
        var newEndUtc = GetEffectiveBlockEnd(bookingRecord);

        var overlappingRecords = await _hireBookingRecordRepository.Table
            .Where(record => record.ProductId == bookingRecord.ProductId)
            .Where(record => !existingRecordId.HasValue || record.Id != existingRecordId.Value)
            .Where(record => !record.IsCancelled)
            .Where(record => record.StatusId == (int)HireAvailabilityState.FullyBooked || record.StatusId == (int)HireAvailabilityState.Unavailable)
            .ToListAsync();

        foreach (var existing in overlappingRecords)
        {
            var existingStartUtc = GetEffectiveBlockStart(existing);
            var existingEndUtc = GetEffectiveBlockEnd(existing);

            if (Intersects(newStartUtc, newEndUtc, existingStartUtc, existingEndUtc))
                return (false, "The selected dates conflict with an existing booking or unavailability period.");
        }

        return (true, string.Empty);
    }

    public async Task<bool> IsSelectionAvailableAsync(int productId, DateTime startUtc, DateTime endUtc, int requestedQuantity = 1)
    {
        if (productId <= 0 || startUtc >= endUtc || requestedQuantity < 1)
            return false;

        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.Deleted || !product.Published)
            return false;

        var requestedStart = GetEffectiveBlockStart(new HireBookingRecord
        {
            StartUtc = startUtc,
            EndUtc = endUtc
        });
        var requestedEnd = GetEffectiveBlockEnd(new HireBookingRecord
        {
            StartUtc = startUtc,
            EndUtc = endUtc
        });

        var records = await _hireBookingRecordRepository.Table
            .Where(record => record.ProductId == productId)
            .Where(record => !record.IsCancelled)
            .Where(record => record.StatusId == (int)HireAvailabilityState.FullyBooked || record.StatusId == (int)HireAvailabilityState.Unavailable)
            .ToListAsync();

        var unavailableQuantity = CalculateUnavailableQuantityForWindow(records, requestedStart, requestedEnd, product.StockQuantity);
        var bookedQuantity = records
            .Where(record => record.Status == HireAvailabilityState.FullyBooked)
            .Where(record => Intersects(requestedStart, requestedEnd, GetEffectiveBlockStart(record), GetEffectiveBlockEnd(record)))
            .Sum(record => Math.Max(record.Quantity, 1));

        var totalStock = Math.Max(product.StockQuantity, 0);
        var affectedQuantity = Math.Min(unavailableQuantity + bookedQuantity, totalStock);
        var remainingQuantity = totalStock - affectedQuantity;

        return remainingQuantity >= requestedQuantity;
    }

    private HireAvailabilityState EvaluateStateForWindow(IList<HireBookingRecord> records, DateTime windowStartUtc, DateTime windowEndUtc, int totalStock, int lowAvailabilityThreshold)
    {
        var unavailableQuantity = CalculateUnavailableQuantityForWindow(records, windowStartUtc, windowEndUtc, totalStock);
        var bookedQuantity = records
            .Where(record => record.Status == HireAvailabilityState.FullyBooked)
            .Where(record => Intersects(windowStartUtc, windowEndUtc, GetEffectiveBlockStart(record), GetEffectiveBlockEnd(record)))
            .Sum(record => Math.Max(record.Quantity, 1));

        var normalizedTotalStock = Math.Max(totalStock, 0);
        var affectedQuantity = Math.Min(unavailableQuantity + bookedQuantity, normalizedTotalStock);
        var remainingQuantity = normalizedTotalStock - affectedQuantity;

        if (remainingQuantity <= 0)
            return unavailableQuantity > 0 ? HireAvailabilityState.Unavailable : HireAvailabilityState.FullyBooked;

        if (remainingQuantity <= lowAvailabilityThreshold)
            return HireAvailabilityState.LowAvailability;

        return HireAvailabilityState.Available;
    }

    private int CalculateBookedQuantityForWindow(IList<HireBookingRecord> records, DateTime windowStartUtc, DateTime windowEndUtc)
    {
        return records
            .Where(record => record.Status == HireAvailabilityState.FullyBooked)
            .Where(record => Intersects(windowStartUtc, windowEndUtc, GetEffectiveBlockStart(record), GetEffectiveBlockEnd(record)))
            .Sum(record => Math.Max(record.Quantity, 1));
    }

    private int CalculateUnavailableQuantityForWindow(IList<HireBookingRecord> records, DateTime windowStartUtc, DateTime windowEndUtc, int totalStock)
    {
        var unavailableQuantity = records
            .Where(record =>
            record.Status == HireAvailabilityState.Unavailable &&
            Intersects(windowStartUtc, windowEndUtc, GetEffectiveBlockStart(record), GetEffectiveBlockEnd(record)))
            .Sum(record => Math.Max(record.Quantity, 1));

        return Math.Min(Math.Max(unavailableQuantity, 0), Math.Max(totalStock, 0));
    }

    public bool RequiresSurchargeReview(DateTime startStoreTime, DateTime endStoreTime)
    {
        var outOfHours = IsOutsideBusinessHours(startStoreTime) || IsOutsideBusinessHours(endStoreTime);
        var sunday = IncludesSunday(startStoreTime, endStoreTime);

        return outOfHours || sunday;
    }

    public async Task InsertAsync(HireBookingRecord bookingRecord)
    {
        bookingRecord.CreatedOnUtc = DateTime.UtcNow;
        bookingRecord.UpdatedOnUtc = bookingRecord.CreatedOnUtc;
        bookingRecord.AdminNote ??= string.Empty;

        await _hireBookingRecordRepository.InsertAsync(bookingRecord);
    }

    public async Task UpdateAsync(HireBookingRecord bookingRecord)
    {
        bookingRecord.UpdatedOnUtc = DateTime.UtcNow;
        bookingRecord.AdminNote ??= string.Empty;

        await _hireBookingRecordRepository.UpdateAsync(bookingRecord);
    }

    public async Task DeleteAsync(HireBookingRecord bookingRecord)
    {
        await _hireBookingRecordRepository.DeleteAsync(bookingRecord);
    }

    private DateTime GetEffectiveBlockStart(HireBookingRecord record)
    {
        var applyBuffer = record.Status == HireAvailabilityState.FullyBooked;
        var (effectiveStartUtc, _) = GetEffectiveBlockWindow(record.StartUtc, record.EndUtc, applyBuffer);
        return effectiveStartUtc;
    }

    private DateTime GetEffectiveBlockEnd(HireBookingRecord record)
    {
        var applyBuffer = record.Status == HireAvailabilityState.FullyBooked;
        var (_, effectiveEndUtc) = GetEffectiveBlockWindow(record.StartUtc, record.EndUtc, applyBuffer);
        return effectiveEndUtc;
    }

    private (DateTime StartUtc, DateTime EndUtc) GetEffectiveBlockWindow(DateTime startUtc, DateTime endUtc, bool applyBuffer)
    {
        var startStoreTime = _dateTimeHelper.ConvertToUserTime(startUtc, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone);
        var endStoreTime = _dateTimeHelper.ConvertToUserTime(endUtc, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone);

        var effectiveStartStoreTime = applyBuffer
            ? startStoreTime.AddHours(-_hireManagementSettings.BufferHoursBefore)
            : startStoreTime;
        var effectiveEndStoreTime = applyBuffer
            ? endStoreTime.AddHours(_hireManagementSettings.BufferHoursAfter)
            : endStoreTime;

        var effectiveStartUtc = _dateTimeHelper.ConvertToUtcTime(DateTime.SpecifyKind(effectiveStartStoreTime, DateTimeKind.Unspecified), _dateTimeHelper.DefaultStoreTimeZone);
        var effectiveEndUtc = _dateTimeHelper.ConvertToUtcTime(DateTime.SpecifyKind(effectiveEndStoreTime, DateTimeKind.Unspecified), _dateTimeHelper.DefaultStoreTimeZone);

        return (effectiveStartUtc, effectiveEndUtc);
    }

    private (DateTime StartUtc, DateTime EndUtc) BuildDurationWindowUtc(DateTime startStoreDate, int durationDays)
    {
        var normalizedDuration = durationDays < 1 ? 1 : durationDays;

        var (startOpens, _) = GetOperatingWindow(startStoreDate.DayOfWeek);
        var endStoreDate = startStoreDate.AddDays(normalizedDuration - 1);
        var (_, endCloses) = GetOperatingWindow(endStoreDate.DayOfWeek);

        var startStoreTime = DateTime.SpecifyKind(startStoreDate.Date.Add(startOpens), DateTimeKind.Unspecified);
        var endStoreTime = DateTime.SpecifyKind(endStoreDate.Date.Add(endCloses), DateTimeKind.Unspecified);

        var startUtc = _dateTimeHelper.ConvertToUtcTime(startStoreTime, _dateTimeHelper.DefaultStoreTimeZone);
        var endUtc = _dateTimeHelper.ConvertToUtcTime(endStoreTime, _dateTimeHelper.DefaultStoreTimeZone);

        return (startUtc, endUtc);
    }

    private bool IsOutsideBusinessHours(DateTime localDateTime)
    {
        var (opens, closes) = GetOperatingWindow(localDateTime.DayOfWeek);
        var time = localDateTime.TimeOfDay;
        return time < opens || time > closes;
    }

    private (TimeSpan Opens, TimeSpan Closes) GetOperatingWindow(DayOfWeek dayOfWeek)
    {
        var open = dayOfWeek switch
        {
            DayOfWeek.Monday => _hireManagementSettings.MondayOpen,
            DayOfWeek.Tuesday => _hireManagementSettings.TuesdayOpen,
            DayOfWeek.Wednesday => _hireManagementSettings.WednesdayOpen,
            DayOfWeek.Thursday => _hireManagementSettings.ThursdayOpen,
            DayOfWeek.Friday => _hireManagementSettings.FridayOpen,
            DayOfWeek.Saturday => _hireManagementSettings.SaturdayOpen,
            _ => _hireManagementSettings.SundayOpen
        };

        var close = dayOfWeek switch
        {
            DayOfWeek.Monday => _hireManagementSettings.MondayClose,
            DayOfWeek.Tuesday => _hireManagementSettings.TuesdayClose,
            DayOfWeek.Wednesday => _hireManagementSettings.WednesdayClose,
            DayOfWeek.Thursday => _hireManagementSettings.ThursdayClose,
            DayOfWeek.Friday => _hireManagementSettings.FridayClose,
            DayOfWeek.Saturday => _hireManagementSettings.SaturdayClose,
            _ => _hireManagementSettings.SundayClose
        };

        if (!TryParseTime(open, out var opens))
            opens = TimeSpan.Zero;

        if (!TryParseTime(close, out var closes))
            closes = new TimeSpan(23, 59, 0);

        return (opens, closes);
    }

    private static bool TryParseTime(string value, out TimeSpan time)
    {
        return TimeSpan.TryParseExact(value, [@"hh\:mm", @"h\:mm", @"hh\:mm\:ss"], CultureInfo.InvariantCulture, out time);
    }

    private static bool IncludesSunday(DateTime startLocal, DateTime endLocal)
    {
        var day = startLocal.Date;
        var endDate = endLocal.Date;

        while (day <= endDate)
        {
            if (day.DayOfWeek == DayOfWeek.Sunday)
                return true;

            day = day.AddDays(1);
        }

        return false;
    }

    private static bool Intersects(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
    {
        return aStart <= bEnd && bStart <= aEnd;
    }
}