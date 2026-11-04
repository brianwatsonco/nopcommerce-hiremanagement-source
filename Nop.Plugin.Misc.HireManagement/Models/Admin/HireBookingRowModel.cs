using Nop.Plugin.Misc.HireManagement.Domain;

namespace Nop.Plugin.Misc.HireManagement.Models.Admin;

public record HireBookingRowModel
{
    public int Id { get; init; }

    public HireAvailabilityState Status { get; init; }

    public DateTime StartUtc { get; init; }

    public DateTime EndUtc { get; init; }

    public int? CustomerId { get; init; }

    public string CustomerEmail { get; init; } = string.Empty;

    public int? OrderId { get; init; }

    public int? OrderItemId { get; init; }

    public decimal PaidAmount { get; init; }

    public int Quantity { get; init; }

    public string AdminNote { get; init; } = string.Empty;

    public bool IsSurchargeReviewRequired { get; init; }
}