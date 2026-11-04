using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.HireManagement.Models.Front;
using Nop.Plugin.Misc.HireManagement.Services;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Web.Framework.Controllers;
using System.Globalization;

namespace Nop.Plugin.Misc.HireManagement.Controllers;

public class HirePublicController : BasePluginController
{
    private const int MaxDurationDays = 90;
    private const int PastYearsAllowed = 1;
    private const int FutureYearsAllowed = 2;

    private readonly IHireBookingService _hireBookingService;
    private readonly IProductService _productService;
    private readonly IDateTimeHelper _dateTimeHelper;

    public HirePublicController(IHireBookingService hireBookingService,
        IProductService productService,
        IDateTimeHelper dateTimeHelper)
    {
        _hireBookingService = hireBookingService;
        _productService = productService;
        _dateTimeHelper = dateTimeHelper;
    }

    public async Task<IActionResult> Availability(int productId = 0, int? year = null, int? month = null, int durationDays = 1)
    {
        var nowStore = _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone);
        var minYear = nowStore.Year - PastYearsAllowed;
        var maxYear = nowStore.Year + FutureYearsAllowed;

        var requestedYear = year ?? nowStore.Year;
        var boundedYear = requestedYear < minYear || requestedYear > maxYear
            ? nowStore.Year
            : requestedYear;

        var requestedMonth = month ?? nowStore.Month;
        var boundedMonth = requestedMonth is < 1 or > 12
            ? nowStore.Month
            : requestedMonth;

        var boundedDurationDays = durationDays < 1
            ? 1
            : Math.Min(durationDays, MaxDurationDays);

        var request = new HireAvailabilityRequestModel
        {
            ProductId = productId,
            Year = boundedYear,
            Month = boundedMonth,
            DurationDays = boundedDurationDays
        };

        var model = new HireAvailabilityViewModel
        {
            Request = request
        };

        if (request.ProductId <= 0)
            return View("~/Plugins/Misc.HireManagement/Views/Public/Availability.cshtml", model);

        var product = await _productService.GetProductByIdAsync(request.ProductId);
        if (product == null || product.Deleted || !product.Published)
            return View("~/Plugins/Misc.HireManagement/Views/Public/Availability.cshtml", model);

        model.ProductFound = true;
        model.ProductName = product.Name;
        model.Days = await _hireBookingService.GetMonthlyAvailabilityForDurationAsync(request.ProductId, request.Year, request.Month, request.DurationDays);

        return View("~/Plugins/Misc.HireManagement/Views/Public/Availability.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ValidateRentalSelection(int productId, string startDateTime, string endDateTime)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.Deleted || !product.Published)
            return Json(new { isAvailable = false, message = "Product is not available." });

        if (!TryParseStoreDateTime(startDateTime, out var parsedStartStoreTime) || !TryParseStoreDateTime(endDateTime, out var parsedEndStoreTime))
            return Json(new { isAvailable = false, message = "Select a valid rental start and end date/time." });

        if (parsedEndStoreTime <= parsedStartStoreTime)
            return Json(new { isAvailable = false, message = "End date/time must be later than start date/time." });

        var nowStore = _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone);
        if (parsedStartStoreTime < nowStore)
            return Json(new { isAvailable = false, message = "Start date/time cannot be in the past." });

        if (product.StockQuantity <= 0)
            return Json(new { isAvailable = false, message = "This product is currently unavailable for the selected dates." });

        var startUtc = _dateTimeHelper.ConvertToUtcTime(DateTime.SpecifyKind(parsedStartStoreTime, DateTimeKind.Unspecified), _dateTimeHelper.DefaultStoreTimeZone);
        var endUtc = _dateTimeHelper.ConvertToUtcTime(DateTime.SpecifyKind(parsedEndStoreTime, DateTimeKind.Unspecified), _dateTimeHelper.DefaultStoreTimeZone);

        var isAvailable = await _hireBookingService.IsSelectionAvailableAsync(productId, startUtc, endUtc);

        return Json(new
        {
            isAvailable,
            message = isAvailable
                ? string.Empty
                : "Those dates are fully booked or unavailable, including buffer time. Please choose different dates."
        });
    }

    [HttpGet]
    public async Task<IActionResult> MonthlyAvailability(int productId, int year, int month, int durationDays = 1)
    {
        if (productId <= 0 || month is < 1 or > 12)
            return Json(new { success = false, days = Array.Empty<object>() });

        var nowStore = _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone);
        var minYear = nowStore.Year - PastYearsAllowed;
        var maxYear = nowStore.Year + FutureYearsAllowed;
        if (year < minYear || year > maxYear)
            return Json(new { success = false, days = Array.Empty<object>() });

        if (durationDays < 1 || durationDays > MaxDurationDays)
            return Json(new { success = false, days = Array.Empty<object>() });

        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null || product.Deleted || !product.Published)
            return Json(new { success = false, days = Array.Empty<object>() });

        var days = await _hireBookingService.GetMonthlyAvailabilityForDurationAsync(productId, year, month, durationDays);

        return Json(new
        {
            success = true,
            days = days.Select(day => new
            {
                date = day.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                state = day.State.ToString(),
                stateId = (int)day.State
            })
        });
    }

    private static bool TryParseStoreDateTime(string value, out DateTime parsed)
    {
        return DateTime.TryParseExact(value,
                   ["yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss"],
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.None,
                   out parsed)
               || DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed);
    }
}