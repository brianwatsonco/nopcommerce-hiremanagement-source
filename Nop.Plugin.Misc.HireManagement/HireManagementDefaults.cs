namespace Nop.Plugin.Misc.HireManagement;

/// <summary>
/// Represents plugin constants
/// </summary>
public class HireManagementDefaults
{
    /// <summary>
    /// Gets a plugin system name
    /// </summary>
    public static string SystemName => "Misc.HireManagement";

    /// <summary>
    /// Gets the configuration route name
    /// </summary>
    public static string ConfigurationRouteName => "Plugin.Misc.HireManagement.Configure";

    /// <summary>
    /// Gets the public availability route name
    /// </summary>
    public static string PublicAvailabilityRouteName => "Plugin.Misc.HireManagement.Public.Availability";

    /// <summary>
    /// Gets the public rental validation route name
    /// </summary>
    public static string PublicValidateRentalSelectionRouteName => "Plugin.Misc.HireManagement.Public.ValidateRentalSelection";

    /// <summary>
    /// Gets the public monthly availability JSON route name
    /// </summary>
    public static string PublicMonthlyAvailabilityRouteName => "Plugin.Misc.HireManagement.Public.MonthlyAvailability";
}