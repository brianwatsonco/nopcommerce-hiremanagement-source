using Nop.Core;

namespace Nop.Plugin.Misc.HireManagement.Domain;

public class HireBookingRecord : BaseEntity
{
    public int ProductId { get; set; }

    public int StatusId { get; set; }

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }

    public int? CustomerId { get; set; }

    public int? OrderId { get; set; }

    public int? OrderItemId { get; set; }

    public decimal PaidAmount { get; set; }

    public int Quantity { get; set; }

    public string AdminNote { get; set; }

    public bool IsCancelled { get; set; }

    public DateTime? CancelledOnUtc { get; set; }

    public string CancellationNote { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    public HireAvailabilityState Status
    {
        get => (HireAvailabilityState)StatusId;
        set => StatusId = (int)value;
    }
}