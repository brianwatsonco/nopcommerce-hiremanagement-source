using Nop.Plugin.Misc.HireManagement.Domain;

namespace Nop.Plugin.Misc.HireManagement.Models;

public partial record HireAvailabilityDay
{
    public DateTime Date { get; init; }

    public HireAvailabilityState State { get; init; }

    public int TotalQuantity { get; init; }

    public int AffectedQuantity { get; init; }

    public int RemainingQuantity { get; init; }
}