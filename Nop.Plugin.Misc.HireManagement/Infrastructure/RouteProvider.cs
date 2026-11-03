using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.HireManagement.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(HireManagementDefaults.ConfigurationRouteName,
            "Admin/HireManagement/Configure",
            new { controller = "HireManagement", action = "Configure", area = AreaNames.ADMIN });

        endpointRouteBuilder.MapControllerRoute(HireManagementDefaults.PublicAvailabilityRouteName,
            "hire/availability/{productId?}",
            new { controller = "HirePublic", action = "Availability" });

        endpointRouteBuilder.MapControllerRoute(HireManagementDefaults.PublicValidateRentalSelectionRouteName,
            "hire/validate-selection",
            new { controller = "HirePublic", action = "ValidateRentalSelection" });

        endpointRouteBuilder.MapControllerRoute(HireManagementDefaults.PublicMonthlyAvailabilityRouteName,
            "hire/monthly-availability",
            new { controller = "HirePublic", action = "MonthlyAvailability" });
    }

    public int Priority => 0;
}