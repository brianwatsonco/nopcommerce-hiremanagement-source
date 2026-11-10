using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using System.Globalization;

namespace Nop.Plugin.Misc.HireManagement.Services;

public class HireProductAttributeParser : ProductAttributeParser
{
    public HireProductAttributeParser(ICurrencyService currencyService,
        IDownloadService downloadService,
        ILocalizationService localizationService,
        IProductAttributeService productAttributeService,
        IRepository<ProductAttributeValue> productAttributeValueRepository,
        IWorkContext workContext)
        : base(currencyService,
              downloadService,
              localizationService,
              productAttributeService,
              productAttributeValueRepository,
              workContext)
    {
    }

    public override void ParseRentalDates(Product product, IFormCollection form, out DateTime? startDate, out DateTime? endDate)
    {
        startDate = null;
        endDate = null;

        if (!product.IsRental)
            return;

        var startDateTimeValue = form[$"rental_start_datetime_{product.Id}"].ToString();
        var endDateTimeValue = form[$"rental_end_datetime_{product.Id}"].ToString();

        if (TryParseDateTime(startDateTimeValue, out var parsedStart))
            startDate = DateTime.SpecifyKind(parsedStart, DateTimeKind.Unspecified);

        if (TryParseDateTime(endDateTimeValue, out var parsedEnd))
            endDate = DateTime.SpecifyKind(parsedEnd, DateTimeKind.Unspecified);

        if (startDate.HasValue && endDate.HasValue)
            return;

        base.ParseRentalDates(product, form, out var fallbackStartDate, out var fallbackEndDate);
        startDate ??= fallbackStartDate;
        endDate ??= fallbackEndDate;
    }

    private static bool TryParseDateTime(string value, out DateTime parsedDate)
    {
        return DateTime.TryParseExact(value,
                   ["yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss"],
                   CultureInfo.InvariantCulture,
                   DateTimeStyles.None,
                   out parsedDate)
               || DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsedDate)
               || DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
    }
}
