using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.HireManagement;

public class HireManagementSettings : ISettings
{
    public int BufferHoursBefore { get; set; } = 0;

    public int BufferHoursAfter { get; set; } = 0;

    public string MondayOpen { get; set; } = "00:00";
    public string MondayClose { get; set; } = "23:59";

    public string TuesdayOpen { get; set; } = "00:00";
    public string TuesdayClose { get; set; } = "23:59";

    public string WednesdayOpen { get; set; } = "00:00";
    public string WednesdayClose { get; set; } = "23:59";

    public string ThursdayOpen { get; set; } = "00:00";
    public string ThursdayClose { get; set; } = "23:59";

    public string FridayOpen { get; set; } = "00:00";
    public string FridayClose { get; set; } = "23:59";

    public string SaturdayOpen { get; set; } = "00:00";
    public string SaturdayClose { get; set; } = "23:59";

    public string SundayOpen { get; set; } = "00:00";
    public string SundayClose { get; set; } = "23:59";
}