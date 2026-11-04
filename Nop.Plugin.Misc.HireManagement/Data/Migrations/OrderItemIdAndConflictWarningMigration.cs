using FluentMigrator;
using Nop.Data;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.HireManagement.Domain;
using Nop.Web.Framework.Extensions;

namespace Nop.Plugin.Misc.HireManagement.Data.Migrations;

[NopMigration("2026-11-04 00:00:00", "Nop.Plugin.Misc.HireManagement add order item id and conflict warning localization", MigrationProcessType.Update)]
public class OrderItemIdAndConflictWarningMigration : MigrationBase
{
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        var tableName = NameCompatibilityManager.GetTableName(typeof(HireBookingRecord));

        if (!Schema.Table(tableName).Column(nameof(HireBookingRecord.OrderItemId)).Exists())
        {
            Alter.Table(tableName)
                .AddColumn(nameof(HireBookingRecord.OrderItemId))
                .AsInt32().Nullable();
        }

        this.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Plugins.Misc.HireManagement.Fields.OrderItemId"] = "Order item ID",
            ["Plugins.Misc.HireManagement.Messages.PotentialConflictSaved"] = "Potential conflict detected with an existing booking/unavailability record. This record was saved because admin conflict blocking is disabled."
        });
    }

    public override void Down()
    {
    }
}
