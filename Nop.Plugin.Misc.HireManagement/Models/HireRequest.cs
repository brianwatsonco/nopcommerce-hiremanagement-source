using System;

namespace Nop.Plugin.Misc.HireManagement.Models;

public partial record HireRequest
{
    public int ProductId { get; init; }

    public DateTime StartUtc { get; init; }

    public DateTime EndUtc { get; init; }

    public decimal OneDayHirePrice { get; init; }
}
