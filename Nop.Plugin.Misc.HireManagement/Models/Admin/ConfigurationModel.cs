using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.HireManagement.Models.Admin;

public record ConfigurationModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.ProductSearchTerm")]
    public string ProductSearchTerm { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.ProductId")]
    public int ProductId { get; set; }

    public IList<SelectListItem> AvailableProducts { get; set; } = [];

    public string ProductName { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.Year")]
    public int Year { get; set; }

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.Month")]
    public int Month { get; set; }

    public IList<HireDiaryDayModel> DiaryDays { get; set; } = [];

    public IList<HireBookingRowModel> ExistingRecords { get; set; } = [];

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.RecordId")]
    public int RecordId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.Status")]
    public int StatusId { get; set; }

    public IList<SelectListItem> AvailableStatuses { get; set; } = [];

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.StartUtc")]
    public DateTime StartUtc { get; set; }

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.EndUtc")]
    public DateTime EndUtc { get; set; }

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.CustomerId")]
    public int? CustomerId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.OrderId")]
    public int? OrderId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.OrderItemId")]
    public int? OrderItemId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.PaidAmount")]
    public decimal PaidAmount { get; set; }

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.Quantity")]
    public int Quantity { get; set; } = 1;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.AdminNote")]
    public string AdminNote { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.BufferHoursBefore")]
    public int BufferHoursBefore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.BufferHoursAfter")]
    public int BufferHoursAfter { get; set; }

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.MondayOpen")]
    public string MondayOpen { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.MondayClose")]
    public string MondayClose { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.TuesdayOpen")]
    public string TuesdayOpen { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.TuesdayClose")]
    public string TuesdayClose { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.WednesdayOpen")]
    public string WednesdayOpen { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.WednesdayClose")]
    public string WednesdayClose { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.ThursdayOpen")]
    public string ThursdayOpen { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.ThursdayClose")]
    public string ThursdayClose { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.FridayOpen")]
    public string FridayOpen { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.FridayClose")]
    public string FridayClose { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.SaturdayOpen")]
    public string SaturdayOpen { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.SaturdayClose")]
    public string SaturdayClose { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.SundayOpen")]
    public string SundayOpen { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Fields.SundayClose")]
    public string SundayClose { get; set; } = string.Empty;

    public bool IsSurchargeReviewRequired { get; set; }

    public string SurchargeReviewNote { get; set; } = string.Empty;
}