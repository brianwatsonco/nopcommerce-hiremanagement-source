using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.HireManagement.Domain;

namespace Nop.Plugin.Misc.HireManagement.Data.Mapping.Builders;

public class HireBookingRecordBuilder : NopEntityBuilder<HireBookingRecord>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table.WithColumn(nameof(HireBookingRecord.ProductId)).AsInt32().ForeignKey<Product>();
        table.WithColumn(nameof(HireBookingRecord.CustomerId)).AsInt32().Nullable().ForeignKey<Customer>();
        table.WithColumn(nameof(HireBookingRecord.OrderId)).AsInt32().Nullable().ForeignKey<Order>();
    }
}