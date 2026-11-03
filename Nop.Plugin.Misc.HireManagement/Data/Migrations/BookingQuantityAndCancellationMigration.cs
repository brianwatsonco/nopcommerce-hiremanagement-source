using FluentMigrator;
using Nop.Data;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.HireManagement.Domain;

namespace Nop.Plugin.Misc.HireManagement.Data.Migrations;

[NopMigration("2026-11-02 00:00:00", "Nop.Plugin.Misc.HireManagement add booking quantity and cancellation audit", MigrationProcessType.Update)]
public class BookingQuantityAndCancellationMigration : MigrationBase
{
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        var tableName = NameCompatibilityManager.GetTableName(typeof(HireBookingRecord));

        if (!Schema.Table(tableName).Column(nameof(HireBookingRecord.Quantity)).Exists())
        {
            Alter.Table(tableName)
                .AddColumn(nameof(HireBookingRecord.Quantity))
                .AsInt32().NotNullable().SetExistingRowsTo(1);
        }

        if (!Schema.Table(tableName).Column(nameof(HireBookingRecord.IsCancelled)).Exists())
        {
            Alter.Table(tableName)
                .AddColumn(nameof(HireBookingRecord.IsCancelled))
                .AsBoolean().NotNullable().SetExistingRowsTo(false);
        }

        if (!Schema.Table(tableName).Column(nameof(HireBookingRecord.CancelledOnUtc)).Exists())
        {
            Alter.Table(tableName)
                .AddColumn(nameof(HireBookingRecord.CancelledOnUtc))
                .AsDateTime2().Nullable();
        }

        if (!Schema.Table(tableName).Column(nameof(HireBookingRecord.CancellationNote)).Exists())
        {
            Alter.Table(tableName)
                .AddColumn(nameof(HireBookingRecord.CancellationNote))
                .AsString().Nullable();
        }
    }

    public override void Down()
    {
    }
}
