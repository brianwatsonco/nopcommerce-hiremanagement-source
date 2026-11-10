using FluentMigrator;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Web.Framework.Extensions;

namespace Nop.Plugin.Misc.HireManagement.Data.Migrations;

[NopMigration("2026-11-03 00:00:00", "Nop.Plugin.Misc.HireManagement update localization resources", MigrationProcessType.Update)]
public class LocalizationUpdateMigration : MigrationBase
{
    public override void Up()
    {
        if (!DataSettingsManager.IsDatabaseInstalled())
            return;

        this.AddOrUpdateLocaleResource(new Dictionary<string, string>
        {
            ["Plugins.Misc.HireManagement.Diary"] = "Hire diary",
            ["Plugins.Misc.HireManagement.ManageEntry"] = "Manage booking/unavailability",
            ["Plugins.Misc.HireManagement.Settings"] = "Hire configuration",
            ["Plugins.Misc.HireManagement.Product"] = "Product",
            ["Plugins.Misc.HireManagement.ExistingRecords"] = "Existing records",
            ["Plugins.Misc.HireManagement.Actions.LoadDiary"] = "Load diary",
            ["Plugins.Misc.HireManagement.Actions.SaveRecord"] = "Save record",
            ["Plugins.Misc.HireManagement.Actions.DeleteRecord"] = "Delete record",
            ["Plugins.Misc.HireManagement.Fields.ProductSearchTerm"] = "Product search",
            ["Plugins.Misc.HireManagement.Fields.ProductId"] = "Product ID",
            ["Plugins.Misc.HireManagement.Fields.Year"] = "Year",
            ["Plugins.Misc.HireManagement.Fields.Month"] = "Month",
            ["Plugins.Misc.HireManagement.Fields.RecordId"] = "Record ID (set > 0 to update)",
            ["Plugins.Misc.HireManagement.Fields.Status"] = "Status",
            ["Plugins.Misc.HireManagement.Fields.StartUtc"] = "Start (store time)",
            ["Plugins.Misc.HireManagement.Fields.EndUtc"] = "End (store time)",
            ["Plugins.Misc.HireManagement.Fields.CustomerId"] = "Customer ID",
            ["Plugins.Misc.HireManagement.Fields.Customer"] = "Customer",
            ["Plugins.Misc.HireManagement.Fields.OrderId"] = "Order ID",
            ["Plugins.Misc.HireManagement.Fields.PaidAmount"] = "Paid amount",
            ["Plugins.Misc.HireManagement.Fields.Quantity"] = "Quantity",
            ["Plugins.Misc.HireManagement.Fields.SurchargeReview"] = "Surcharge review",
            ["Plugins.Misc.HireManagement.Fields.AdminNote"] = "Admin note",
            ["Plugins.Misc.HireManagement.Fields.BufferHoursBefore"] = "Buffer hours before hire",
            ["Plugins.Misc.HireManagement.Fields.BufferHoursAfter"] = "Buffer hours after hire",
            ["Plugins.Misc.HireManagement.Fields.MondayOpen"] = "Monday opening time (HH:mm)",
            ["Plugins.Misc.HireManagement.Fields.MondayClose"] = "Monday closing time (HH:mm)",
            ["Plugins.Misc.HireManagement.Fields.TuesdayOpen"] = "Tuesday opening time (HH:mm)",
            ["Plugins.Misc.HireManagement.Fields.TuesdayClose"] = "Tuesday closing time (HH:mm)",
            ["Plugins.Misc.HireManagement.Fields.WednesdayOpen"] = "Wednesday opening time (HH:mm)",
            ["Plugins.Misc.HireManagement.Fields.WednesdayClose"] = "Wednesday closing time (HH:mm)",
            ["Plugins.Misc.HireManagement.Fields.ThursdayOpen"] = "Thursday opening time (HH:mm)",
            ["Plugins.Misc.HireManagement.Fields.ThursdayClose"] = "Thursday closing time (HH:mm)",
            ["Plugins.Misc.HireManagement.Fields.FridayOpen"] = "Friday opening time (HH:mm)",
            ["Plugins.Misc.HireManagement.Fields.FridayClose"] = "Friday closing time (HH:mm)",
            ["Plugins.Misc.HireManagement.Fields.SaturdayOpen"] = "Saturday opening time (HH:mm)",
            ["Plugins.Misc.HireManagement.Fields.SaturdayClose"] = "Saturday closing time (HH:mm)",
            ["Plugins.Misc.HireManagement.Fields.SundayOpen"] = "Sunday opening time (HH:mm)",
            ["Plugins.Misc.HireManagement.Fields.SundayClose"] = "Sunday closing time (HH:mm)",
            ["Plugins.Misc.HireManagement.Messages.ProductNotFound"] = "Product not found.",
            ["Plugins.Misc.HireManagement.Messages.RecordNotFound"] = "Record not found.",
            ["Plugins.Misc.HireManagement.Messages.RecordIdNotFoundCreated"] = "Record ID was not found. A new record has been created using the entered details.",
            ["Plugins.Misc.HireManagement.Messages.RecordAdded"] = "Hire record added.",
            ["Plugins.Misc.HireManagement.Messages.RecordUpdated"] = "Hire record updated.",
            ["Plugins.Misc.HireManagement.Messages.RecordDeleted"] = "Hire record deleted.",
            ["Plugins.Misc.HireManagement.Messages.SurchargeReviewRequired"] = "This booking includes out-of-hours or Sunday timing. Review and agree any surcharge with the customer.",
            ["Plugins.Misc.HireManagement.Front.Title"] = "Hire availability",
            ["Plugins.Misc.HireManagement.Front.Fields.ProductId"] = "Product ID",
            ["Plugins.Misc.HireManagement.Front.Fields.Year"] = "Year",
            ["Plugins.Misc.HireManagement.Front.Fields.Month"] = "Month",
            ["Plugins.Misc.HireManagement.Front.Fields.DurationDays"] = "Hire duration (days)",
            ["Plugins.Misc.HireManagement.Front.Actions.Load"] = "Load availability",
            ["Plugins.Misc.HireManagement.Front.Messages.EnterProduct"] = "Enter a product ID to load availability.",
            ["Plugins.Misc.HireManagement.Front.Messages.ProductNotFound"] = "Product not found or not published.",
            ["Plugins.Misc.HireManagement.Front.Messages.PricingGuidance"] = "Duration options are read from the product's day-related attribute values as reference labels. The plugin ignores that attribute during add-to-cart to prevent additive 100%+ pricing behavior.",
            ["Plugins.Misc.HireManagement.Front.Legend.Available"] = "Available",
            ["Plugins.Misc.HireManagement.Front.Legend.LowAvailability"] = "Low availability",
            ["Plugins.Misc.HireManagement.Front.Legend.FullyBooked"] = "Fully booked",
            ["Plugins.Misc.HireManagement.Front.Legend.Unavailable"] = "Unavailable"
        });
    }

    public override void Down()
    {
    }
}
