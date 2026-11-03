using Nop.Plugin.Misc.HireManagement.Models;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.HireManagement.Models.Front;

public record HireAvailabilityViewModel : BaseNopModel
{
    public HireAvailabilityRequestModel Request { get; init; } = new();

    public string ProductName { get; set; } = string.Empty;

    public bool ProductFound { get; set; }

    public IList<HireAvailabilityDay> Days { get; set; } = [];
}