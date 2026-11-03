using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Misc.HireManagement.Services;

public interface IHireOrderSyncService
{
    Task<int> SyncOrderAsync(Order order);

    Task<int> ReleaseOrderAsync(Order order, string reason);

    Task<int> SyncProductMonthAsync(int productId, int year, int month);
}