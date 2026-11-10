using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.HireManagement.Domain;
using Nop.Plugin.Misc.HireManagement.Models.Admin;
using Nop.Plugin.Misc.HireManagement.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Globalization;

namespace Nop.Plugin.Misc.HireManagement.Controllers;

[AutoValidateAntiforgeryToken]
[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
public class HireManagementController : BasePluginController
{
    private readonly IHireBookingService _hireBookingService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ISettingService _settingService;
    private readonly IHireOrderSyncService _hireOrderSyncService;

    public HireManagementController(IHireBookingService hireBookingService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IOrderService orderService,
        IProductService productService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        ISettingService settingService,
        IHireOrderSyncService hireOrderSyncService)
    {
        _hireBookingService = hireBookingService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _orderService = orderService;
        _productService = productService;
        _customerService = customerService;
        _dateTimeHelper = dateTimeHelper;
        _settingService = settingService;
        _hireOrderSyncService = hireOrderSyncService;
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> Configure(int productId = 0, int? year = null, int? month = null, string productSearchTerm = "", int? editRecordId = null)
    {
        var nowStoreTime = _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone);
        var settings = await _settingService.LoadSettingAsync<HireManagementSettings>();
        var (opens, closes) = GetOperatingWindowForDate(settings, nowStoreTime.Date);

        var model = new ConfigurationModel
        {
            ProductId = productId,
            ProductSearchTerm = productSearchTerm ?? string.Empty,
            Year = year ?? nowStoreTime.Year,
            Month = month ?? nowStoreTime.Month,
            StatusId = (int)HireAvailabilityState.FullyBooked,
            StartUtc = nowStoreTime.Date.Add(opens),
            EndUtc = nowStoreTime.Date.Add(closes),
            Quantity = 1
        };

        if (!year.HasValue)
            model.Year = nowStoreTime.Year;

        if (!month.HasValue)
            model.Month = nowStoreTime.Month;

        MapSettingsToModel(settings, model);

        PrepareStatusList(model);
        await PopulateAvailableProductsAsync(model);

        if (editRecordId.HasValue && editRecordId.Value > 0)
            await PopulateEditRecordAsync(model, editRecordId.Value);

        if (model.ProductId > 0)
        {
            var product = await _productService.GetProductByIdAsync(model.ProductId);
            model.ProductName = product?.Name ?? string.Empty;

            if (product != null)
                await PopulateDiaryAndRowsAsync(model, product);
        }

        return View("~/Plugins/Misc.HireManagement/Views/Admin/Configure.cshtml", model);
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> EditRecord(int recordId, int productId, int year, int month, string productSearchTerm = "")
    {
        return await Configure(productId, year, month, productSearchTerm, recordId);
    }

    [HttpPost, ActionName("Configure")]
    [FormValueRequired("save-record")]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> SaveRecord(ConfigurationModel model)
    {
        PrepareStatusList(model);
        await PopulateAvailableProductsAsync(model);

        var product = await _productService.GetProductByIdAsync(model.ProductId);
        if (product == null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Misc.HireManagement.Messages.ProductNotFound"));
            return await Configure(model.ProductId, model.Year, model.Month);
        }

        if (model.Quantity < 1)
            model.Quantity = 1;

        if (model.Quantity > product.StockQuantity)
        {
            _notificationService.ErrorNotification($"Quantity cannot exceed current stock ({product.StockQuantity}).");
            await PopulateDiaryAndRowsAsync(model, product);
            return View("~/Plugins/Misc.HireManagement/Views/Admin/Configure.cshtml", model);
        }

        var normalizedStartUtc = ConvertStoreTimeToUtc(model.StartUtc);
        var normalizedEndUtc = ConvertStoreTimeToUtc(model.EndUtc);

        var requiresSurchargeReview = _hireBookingService.RequiresSurchargeReview(model.StartUtc, model.EndUtc);
        model.IsSurchargeReviewRequired = requiresSurchargeReview;
        model.SurchargeReviewNote = requiresSurchargeReview
            ? await _localizationService.GetResourceAsync("Plugins.Misc.HireManagement.Messages.SurchargeReviewRequired")
            : string.Empty;

        if (model.RecordId > 0)
        {
            var existingRecord = await _hireBookingService.GetByIdAsync(model.RecordId);
            if (existingRecord == null)
            {
                var newRecordFromMissingId = new HireBookingRecord
                {
                    ProductId = model.ProductId,
                    StatusId = model.StatusId,
                    StartUtc = normalizedStartUtc,
                    EndUtc = normalizedEndUtc,
                    CustomerId = model.CustomerId,
                    OrderId = model.OrderId,
                    PaidAmount = model.PaidAmount,
                    Quantity = model.Quantity,
                    AdminNote = model.AdminNote
                };

                var createValidationResult = await _hireBookingService.ValidateAsync(newRecordFromMissingId);
                if (!createValidationResult.IsValid)
                {
                    _notificationService.ErrorNotification(createValidationResult.Error);
                    await PopulateDiaryAndRowsAsync(model, product);
                    return View("~/Plugins/Misc.HireManagement/Views/Admin/Configure.cshtml", model);
                }

                await _hireBookingService.InsertAsync(newRecordFromMissingId);
                _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Plugins.Misc.HireManagement.Messages.RecordIdNotFoundCreated"));

                if (requiresSurchargeReview)
                    _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Plugins.Misc.HireManagement.Messages.SurchargeReviewRequired"));

                return RedirectToAction(nameof(Configure), new { productId = model.ProductId, year = model.Year, month = model.Month, productSearchTerm = model.ProductSearchTerm });
            }

            existingRecord.ProductId = model.ProductId;
            existingRecord.StatusId = model.StatusId;
            existingRecord.StartUtc = normalizedStartUtc;
            existingRecord.EndUtc = normalizedEndUtc;
            existingRecord.CustomerId = model.CustomerId;
            existingRecord.OrderId = model.OrderId;
            existingRecord.PaidAmount = model.PaidAmount;
            existingRecord.Quantity = model.Quantity;
            existingRecord.AdminNote = model.AdminNote;

            var validationResult = await _hireBookingService.ValidateAsync(existingRecord, existingRecord.Id);
            if (!validationResult.IsValid)
            {
                _notificationService.ErrorNotification(validationResult.Error);
                await PopulateDiaryAndRowsAsync(model, product);
                return View("~/Plugins/Misc.HireManagement/Views/Admin/Configure.cshtml", model);
            }

            await _hireBookingService.UpdateAsync(existingRecord);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.HireManagement.Messages.RecordUpdated"));
        }
        else
        {
            var newRecord = new HireBookingRecord
            {
                ProductId = model.ProductId,
                StatusId = model.StatusId,
                StartUtc = normalizedStartUtc,
                EndUtc = normalizedEndUtc,
                CustomerId = model.CustomerId,
                OrderId = model.OrderId,
                PaidAmount = model.PaidAmount,
                Quantity = model.Quantity,
                AdminNote = model.AdminNote
            };

            var validationResult = await _hireBookingService.ValidateAsync(newRecord);
            if (!validationResult.IsValid)
            {
                _notificationService.ErrorNotification(validationResult.Error);
                await PopulateDiaryAndRowsAsync(model, product);
            return View("~/Plugins/Misc.HireManagement/Views/Admin/Configure.cshtml", model);
            }

            await _hireBookingService.InsertAsync(newRecord);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.HireManagement.Messages.RecordAdded"));
        }

        if (requiresSurchargeReview)
            _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Plugins.Misc.HireManagement.Messages.SurchargeReviewRequired"));

        return RedirectToAction(nameof(Configure), new { productId = model.ProductId, year = model.Year, month = model.Month, productSearchTerm = model.ProductSearchTerm });
    }

    [HttpPost, ActionName("Configure")]
    [FormValueRequired("delete-record")]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> DeleteRecord(ConfigurationModel model)
    {
        if (model.RecordId <= 0)
        {
            _notificationService.ErrorNotification("Enter a Record ID to delete.");
            return RedirectToAction(nameof(Configure), new { productId = model.ProductId, year = model.Year, month = model.Month, productSearchTerm = model.ProductSearchTerm });
        }

        var existingRecord = await _hireBookingService.GetByIdAsync(model.RecordId);
        if (existingRecord == null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Misc.HireManagement.Messages.RecordNotFound"));
            return RedirectToAction(nameof(Configure), new { productId = model.ProductId, year = model.Year, month = model.Month, productSearchTerm = model.ProductSearchTerm });
        }

        await _hireBookingService.DeleteAsync(existingRecord);
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.HireManagement.Messages.RecordDeleted"));

        return RedirectToAction(nameof(Configure), new { productId = model.ProductId, year = model.Year, month = model.Month, productSearchTerm = model.ProductSearchTerm });
    }

    [HttpPost, ActionName("Configure")]
    [FormValueRequired("save-settings")]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> SaveSettings(ConfigurationModel model)
    {
        var settings = await _settingService.LoadSettingAsync<HireManagementSettings>();

        settings.BufferHoursBefore = model.BufferHoursBefore;
        settings.BufferHoursAfter = model.BufferHoursAfter;

        settings.MondayOpen = model.MondayOpen ?? string.Empty;
        settings.MondayClose = model.MondayClose ?? string.Empty;
        settings.TuesdayOpen = model.TuesdayOpen ?? string.Empty;
        settings.TuesdayClose = model.TuesdayClose ?? string.Empty;
        settings.WednesdayOpen = model.WednesdayOpen ?? string.Empty;
        settings.WednesdayClose = model.WednesdayClose ?? string.Empty;
        settings.ThursdayOpen = model.ThursdayOpen ?? string.Empty;
        settings.ThursdayClose = model.ThursdayClose ?? string.Empty;
        settings.FridayOpen = model.FridayOpen ?? string.Empty;
        settings.FridayClose = model.FridayClose ?? string.Empty;
        settings.SaturdayOpen = model.SaturdayOpen ?? string.Empty;
        settings.SaturdayClose = model.SaturdayClose ?? string.Empty;
        settings.SundayOpen = model.SundayOpen ?? string.Empty;
        settings.SundayClose = model.SundayClose ?? string.Empty;

        await _settingService.SaveSettingAsync(settings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return RedirectToAction(nameof(Configure), new { productId = model.ProductId, year = model.Year, month = model.Month, productSearchTerm = model.ProductSearchTerm });
    }

    [HttpPost, ActionName("Configure")]
    [FormValueRequired("sync-order-bookings")]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> SyncOrderBookings(ConfigurationModel model)
    {
        if (model.ProductId <= 0)
        {
            _notificationService.ErrorNotification("Select a product before syncing order bookings.");
            return RedirectToAction(nameof(Configure), new { productId = model.ProductId, year = model.Year, month = model.Month, productSearchTerm = model.ProductSearchTerm });
        }

        var createdCount = await _hireOrderSyncService.SyncProductMonthAsync(model.ProductId, model.Year, model.Month);

        if (createdCount > 0)
            _notificationService.SuccessNotification($"Imported {createdCount} booking record(s) from existing website orders.");
        else
            _notificationService.WarningNotification("No website rental bookings were found to import for the selected product/month.");

        return RedirectToAction(nameof(Configure), new { productId = model.ProductId, year = model.Year, month = model.Month, productSearchTerm = model.ProductSearchTerm });
    }

    private async Task PopulateDiaryAndRowsAsync(ConfigurationModel model, Product product)
    {
        model.ProductName = product.Name;

        await _hireOrderSyncService.SyncProductMonthAsync(model.ProductId, model.Year, model.Month);

        var days = await _hireBookingService.GetMonthlyAvailabilityAsync(model.ProductId, model.Year, model.Month);
        model.DiaryDays = days
            .Select(day => new HireDiaryDayModel
            {
                Date = day.Date,
                State = day.State,
                TotalQuantity = day.TotalQuantity,
                AffectedQuantity = day.AffectedQuantity,
                RemainingQuantity = day.RemainingQuantity
            })
            .ToList();

        var records = await _hireBookingService.GetByProductAndMonthAsync(model.ProductId, model.Year, model.Month);
        var rows = new List<HireBookingRowModel>(records.Count);

        foreach (var record in records)
        {
            var recordStartStoreTime = _dateTimeHelper.ConvertToUserTime(record.StartUtc, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone);
            var recordEndStoreTime = _dateTimeHelper.ConvertToUserTime(record.EndUtc, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone);

            var customerEmail = string.Empty;
            if (record.CustomerId.HasValue)
            {
                var customer = await _customerService.GetCustomerByIdAsync(record.CustomerId.Value);
                customerEmail = customer?.Email ?? string.Empty;
            }

            var paidAmount = record.PaidAmount;
            if (record.OrderId.HasValue)
            {
                var order = await _orderService.GetOrderByIdAsync(record.OrderId.Value);
                if (order != null)
                    paidAmount = order.PaidDateUtc.HasValue ? order.OrderTotal : record.PaidAmount;
            }

            rows.Add(new HireBookingRowModel
            {
                Id = record.Id,
                Status = record.Status,
                StartUtc = recordStartStoreTime,
                EndUtc = recordEndStoreTime,
                CustomerId = record.CustomerId,
                CustomerEmail = customerEmail,
                OrderId = record.OrderId,
                PaidAmount = paidAmount,
                Quantity = record.Quantity,
                AdminNote = record.AdminNote ?? string.Empty,
                IsSurchargeReviewRequired = _hireBookingService.RequiresSurchargeReview(recordStartStoreTime, recordEndStoreTime)
            });
        }

        model.ExistingRecords = rows;
    }

    private async Task PopulateEditRecordAsync(ConfigurationModel model, int recordId)
    {
        var record = await _hireBookingService.GetByIdAsync(recordId);
        if (record == null)
            return;

        model.RecordId = record.Id;
        model.ProductId = record.ProductId;
        model.StatusId = record.StatusId;
        model.StartUtc = _dateTimeHelper.ConvertToUserTime(record.StartUtc, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone);
        model.EndUtc = _dateTimeHelper.ConvertToUserTime(record.EndUtc, TimeZoneInfo.Utc, _dateTimeHelper.DefaultStoreTimeZone);
        model.CustomerId = record.CustomerId;
        model.OrderId = record.OrderId;
        model.PaidAmount = record.PaidAmount;
        model.Quantity = record.Quantity < 1 ? 1 : record.Quantity;
        model.AdminNote = record.AdminNote ?? string.Empty;
        model.IsSurchargeReviewRequired = _hireBookingService.RequiresSurchargeReview(model.StartUtc, model.EndUtc);
        model.SurchargeReviewNote = model.IsSurchargeReviewRequired
            ? await _localizationService.GetResourceAsync("Plugins.Misc.HireManagement.Messages.SurchargeReviewRequired")
            : string.Empty;
    }

    private async Task PopulateAvailableProductsAsync(ConfigurationModel model)
    {
        var products = await _productService.SearchProductsAsync(
            pageIndex: 0,
            pageSize: 50,
            keywords: model.ProductSearchTerm,
            showHidden: true);

        model.AvailableProducts = products
            .Select(product => new SelectListItem
            {
                Value = product.Id.ToString(),
                Text = $"{product.Name} ({product.Id})",
                Selected = product.Id == model.ProductId
            })
            .ToList();

        if (model.ProductId > 0 && model.AvailableProducts.All(item => item.Value != model.ProductId.ToString()))
        {
            var selectedProduct = await _productService.GetProductByIdAsync(model.ProductId);
            if (selectedProduct != null)
            {
                model.AvailableProducts.Insert(0, new SelectListItem
                {
                    Value = selectedProduct.Id.ToString(),
                    Text = $"{selectedProduct.Name} ({selectedProduct.Id})",
                    Selected = true
                });
            }
        }
    }

    private DateTime ConvertStoreTimeToUtc(DateTime storeDateTime)
    {
        var unspecifiedStoreDateTime = DateTime.SpecifyKind(storeDateTime, DateTimeKind.Unspecified);
        return _dateTimeHelper.ConvertToUtcTime(unspecifiedStoreDateTime, _dateTimeHelper.DefaultStoreTimeZone);
    }

    private static void MapSettingsToModel(HireManagementSettings settings, ConfigurationModel model)
    {
        model.BufferHoursBefore = settings.BufferHoursBefore;
        model.BufferHoursAfter = settings.BufferHoursAfter;

        model.MondayOpen = settings.MondayOpen;
        model.MondayClose = settings.MondayClose;
        model.TuesdayOpen = settings.TuesdayOpen;
        model.TuesdayClose = settings.TuesdayClose;
        model.WednesdayOpen = settings.WednesdayOpen;
        model.WednesdayClose = settings.WednesdayClose;
        model.ThursdayOpen = settings.ThursdayOpen;
        model.ThursdayClose = settings.ThursdayClose;
        model.FridayOpen = settings.FridayOpen;
        model.FridayClose = settings.FridayClose;
        model.SaturdayOpen = settings.SaturdayOpen;
        model.SaturdayClose = settings.SaturdayClose;
        model.SundayOpen = settings.SundayOpen;
        model.SundayClose = settings.SundayClose;
    }

    private static (TimeSpan Opens, TimeSpan Closes) GetOperatingWindowForDate(HireManagementSettings settings, DateTime date)
    {
        var open = date.DayOfWeek switch
        {
            DayOfWeek.Monday => settings.MondayOpen,
            DayOfWeek.Tuesday => settings.TuesdayOpen,
            DayOfWeek.Wednesday => settings.WednesdayOpen,
            DayOfWeek.Thursday => settings.ThursdayOpen,
            DayOfWeek.Friday => settings.FridayOpen,
            DayOfWeek.Saturday => settings.SaturdayOpen,
            _ => settings.SundayOpen
        };

        var close = date.DayOfWeek switch
        {
            DayOfWeek.Monday => settings.MondayClose,
            DayOfWeek.Tuesday => settings.TuesdayClose,
            DayOfWeek.Wednesday => settings.WednesdayClose,
            DayOfWeek.Thursday => settings.ThursdayClose,
            DayOfWeek.Friday => settings.FridayClose,
            DayOfWeek.Saturday => settings.SaturdayClose,
            _ => settings.SundayClose
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

    private static void PrepareStatusList(ConfigurationModel model)
    {
        model.AvailableStatuses =
        [
            new SelectListItem
            {
                Value = ((int)HireAvailabilityState.FullyBooked).ToString(),
                Text = HireAvailabilityState.FullyBooked.ToString(),
                Selected = model.StatusId == (int)HireAvailabilityState.FullyBooked
            },
            new SelectListItem
            {
                Value = ((int)HireAvailabilityState.Unavailable).ToString(),
                Text = HireAvailabilityState.Unavailable.ToString(),
                Selected = model.StatusId == (int)HireAvailabilityState.Unavailable
            }
        ];
    }
}