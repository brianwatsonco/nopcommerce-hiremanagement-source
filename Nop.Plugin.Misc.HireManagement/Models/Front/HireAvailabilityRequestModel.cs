using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.HireManagement.Models.Front;

public record HireAvailabilityRequestModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.Misc.HireManagement.Front.Fields.ProductId")]
    public int ProductId { get; set; }

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Front.Fields.Year")]
    public int Year { get; set; }

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Front.Fields.Month")]
    public int Month { get; set; }

    [NopResourceDisplayName("Plugins.Misc.HireManagement.Front.Fields.DurationDays")]
    public int DurationDays { get; set; } = 1;
}