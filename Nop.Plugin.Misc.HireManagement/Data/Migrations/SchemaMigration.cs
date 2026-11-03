using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.HireManagement.Domain;

namespace Nop.Plugin.Misc.HireManagement.Data.Migrations;

[NopMigration("2026-10-27 00:00:00", "Nop.Plugin.Misc.HireManagement schema", MigrationProcessType.Installation)]
public class SchemaMigration : Migration
{
    public override void Up()
    {
        this.CreateTableIfNotExists<HireBookingRecord>();
    }

    public override void Down()
    {
        this.DeleteTableIfExists<HireBookingRecord>();
    }
}