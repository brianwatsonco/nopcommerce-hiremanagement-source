using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.HireManagement.Domain;

namespace Nop.Plugin.Misc.HireManagement.Services;

public class HireOrderSyncService : IHireOrderSyncService
{
    private readonly IRepository<HireBookingRecord> _hireBookingRecordRepository;
    private readonly IRepository<OrderItem> _orderItemRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IHireBookingService _hireBookingService;

    public HireOrderSyncService(IRepository<HireBookingRecord> hireBookingRecordRepository,
        IRepository<OrderItem> orderItemRepository,
        IRepository<Order> orderRepository,
        IHireBookingService hireBookingService)
    {
        _hireBookingRecordRepository = hireBookingRecordRepository;
        _orderItemRepository = orderItemRepository;
        _orderRepository = orderRepository;
        _hireBookingService = hireBookingService;
    }

    public async Task<int> SyncOrderAsync(Order order)
    {
        if (order == null)
            return 0;

        if (order.OrderStatus == OrderStatus.Cancelled)
            return await ReleaseOrderAsync(order, "Order is cancelled.");

        var rentalItems = await _orderItemRepository.Table
            .Where(item => item.OrderId == order.Id && item.RentalStartDateUtc.HasValue && item.RentalEndDateUtc.HasValue)
            .ToListAsync();

        var activeRentalItemIds = rentalItems.Select(item => item.Id).ToHashSet();
        await ReleaseMissingOrderItemsAsync(order, activeRentalItemIds, "Order item removed or rental dates changed.");

        if (!rentalItems.Any())
            return 0;

        var created = 0;

        foreach (var item in rentalItems)
        {
            created += await SyncOrderItemAsync(order, item);
        }

        return created;
    }

    public async Task<int> SyncProductMonthAsync(int productId, int year, int month)
    {
        if (productId <= 0)
            return 0;

        var monthStartUtc = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEndUtc = monthStartUtc.AddMonths(1).AddTicks(-1);

        var rentalItems = await _orderItemRepository.Table
            .Where(item => item.ProductId == productId)
            .Where(item => item.RentalStartDateUtc.HasValue && item.RentalEndDateUtc.HasValue)
            .Where(item => item.RentalStartDateUtc.Value <= monthEndUtc && item.RentalEndDateUtc.Value >= monthStartUtc)
            .ToListAsync();

        if (!rentalItems.Any())
            return 0;

        var orderIds = rentalItems.Select(item => item.OrderId).Distinct().ToList();
        var orders = await _orderRepository.Table
            .Where(order => orderIds.Contains(order.Id))
            .ToDictionaryAsync(order => order.Id);

        var created = 0;

        foreach (var item in rentalItems)
        {
            if (!orders.TryGetValue(item.OrderId, out var order))
                continue;

            if (order.OrderStatus == OrderStatus.Cancelled)
            {
                await ReleaseOrderAsync(order, "Order is cancelled.");
                continue;
            }

            created += await SyncOrderItemAsync(order, item);
        }

        return created;
    }

    public async Task<int> ReleaseOrderAsync(Order order, string reason)
    {
        if (order == null)
            return 0;

        var records = await _hireBookingRecordRepository.Table
            .Where(record => record.OrderId == order.Id)
            .Where(record => !record.IsCancelled)
            .ToListAsync();

        if (!records.Any())
            return 0;

        foreach (var record in records)
        {
            record.IsCancelled = true;
            record.CancelledOnUtc = DateTime.UtcNow;
            record.CancellationNote = reason;
            record.UpdatedOnUtc = DateTime.UtcNow;
        }

        foreach (var record in records)
            await _hireBookingRecordRepository.UpdateAsync(record);

        return records.Count;
    }

    private async Task<int> SyncOrderItemAsync(Order order, OrderItem item)
    {
        var rentalStartUtc = item.RentalStartDateUtc!.Value;
        var rentalEndUtc = item.RentalEndDateUtc!.Value;

        var existingRecord = await _hireBookingRecordRepository.Table
            .FirstOrDefaultAsync(record => record.OrderItemId == item.Id);

        if (existingRecord == null)
        {
            existingRecord = await _hireBookingRecordRepository.Table
                .FirstOrDefaultAsync(record =>
                    record.OrderId == order.Id &&
                    record.ProductId == item.ProductId &&
                    record.StatusId == (int)HireAvailabilityState.FullyBooked &&
                    record.StartUtc == rentalStartUtc &&
                    record.EndUtc == rentalEndUtc);
        }

        if (existingRecord != null)
        {
            var normalizedQuantity = item.Quantity < 1 ? 1 : item.Quantity;
            if (existingRecord.IsCancelled ||
                existingRecord.Quantity != normalizedQuantity ||
                existingRecord.OrderItemId != item.Id ||
                existingRecord.StartUtc != rentalStartUtc ||
                existingRecord.EndUtc != rentalEndUtc ||
                existingRecord.CustomerId != order.CustomerId ||
                existingRecord.OrderId != order.Id ||
                existingRecord.PaidAmount != item.PriceInclTax)
            {
                existingRecord.IsCancelled = false;
                existingRecord.CancelledOnUtc = null;
                existingRecord.CancellationNote = null;
                existingRecord.StartUtc = rentalStartUtc;
                existingRecord.EndUtc = rentalEndUtc;
                existingRecord.CustomerId = order.CustomerId;
                existingRecord.OrderId = order.Id;
                existingRecord.Quantity = normalizedQuantity;
                existingRecord.OrderItemId = item.Id;
                existingRecord.PaidAmount = item.PriceInclTax;
                existingRecord.UpdatedOnUtc = DateTime.UtcNow;

                var updateResult = await _hireBookingService.SaveValidatedAsync(existingRecord, existingRecord.Id, includeConflictCheck: true);
                return updateResult.Saved ? 1 : 0;
            }

            return 0;
        }

        var newRecord = new HireBookingRecord
        {
            ProductId = item.ProductId,
            StatusId = (int)HireAvailabilityState.FullyBooked,
            StartUtc = rentalStartUtc,
            EndUtc = rentalEndUtc,
            CustomerId = order.CustomerId,
            OrderId = order.Id,
            OrderItemId = item.Id,
            PaidAmount = item.PriceInclTax,
            Quantity = item.Quantity < 1 ? 1 : item.Quantity,
            AdminNote = "Created from website order",
            IsCancelled = false,
            CancellationNote = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };

        var insertResult = await _hireBookingService.SaveValidatedAsync(newRecord, includeConflictCheck: true);
        return insertResult.Saved ? 1 : 0;
    }

    private async Task<int> ReleaseMissingOrderItemsAsync(Order order, HashSet<int> activeRentalItemIds, string reason)
    {
        if (order == null)
            return 0;

        var records = await _hireBookingRecordRepository.Table
            .Where(record => record.OrderId == order.Id)
            .Where(record => record.StatusId == (int)HireAvailabilityState.FullyBooked)
            .Where(record => !record.IsCancelled)
            .ToListAsync();

        if (!records.Any())
            return 0;

        var updated = 0;

        foreach (var record in records)
        {
            if (!record.OrderItemId.HasValue)
                continue;

            if (activeRentalItemIds.Contains(record.OrderItemId.Value))
                continue;

            record.IsCancelled = true;
            record.CancelledOnUtc = DateTime.UtcNow;
            record.CancellationNote = reason;
            record.UpdatedOnUtc = DateTime.UtcNow;
            await _hireBookingRecordRepository.UpdateAsync(record);
            updated++;
        }

        return updated;
    }
}