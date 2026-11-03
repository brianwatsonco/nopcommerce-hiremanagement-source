using Nop.Core.Events;
using Nop.Core.Domain.Orders;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.HireManagement.Services;

public class HireOrderEventConsumer :
    IConsumer<OrderPlacedEvent>,
    IConsumer<OrderStatusChangedEvent>,
    IConsumer<EntityDeletedEvent<Order>>
{
    private readonly IHireOrderSyncService _hireOrderSyncService;

    public HireOrderEventConsumer(IHireOrderSyncService hireOrderSyncService)
    {
        _hireOrderSyncService = hireOrderSyncService;
    }

    public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
    {
        var order = eventMessage?.Order;
        if (order == null)
            return;

        await _hireOrderSyncService.SyncOrderAsync(order);
    }

    public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
    {
        var order = eventMessage?.Order;
        if (order == null)
            return;

        if (order.OrderStatus == OrderStatus.Cancelled)
        {
            await _hireOrderSyncService.ReleaseOrderAsync(order, "Order status changed to cancelled.");
            return;
        }

        await _hireOrderSyncService.SyncOrderAsync(order);
    }

    public async Task HandleEventAsync(EntityDeletedEvent<Order> eventMessage)
    {
        var order = eventMessage?.Entity;
        if (order == null)
            return;

        await _hireOrderSyncService.ReleaseOrderAsync(order, "Order deleted.");
    }
}