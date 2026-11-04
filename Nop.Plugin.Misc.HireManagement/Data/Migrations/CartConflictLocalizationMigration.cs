using FluentMigrator;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Web.Framework.Extensions;

namespace Nop.Plugin.Misc.HireManagement.Data.Migrations;

[NopMigration("2026-11-05 00:00:00", "Nop.Plugin.Misc.HireManagement add cart conflict localization", MigrationProcessType.Update)]
public class CartConflictLocalizationMigration : MigrationBase
{
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        this.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Plugins.Misc.HireManagement.Front.Messages.CartConflict"] = "One or more rental items in your cart are no longer available for the selected dates. Please review your rental dates."
        });
    }

    public override void Down()
    {
    }
}
