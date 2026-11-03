using Nop.Data.Mapping;
using Nop.Plugin.Misc.HireManagement.Domain;

namespace Nop.Plugin.Misc.HireManagement.Data.Mapping;

public class HireManagementNameCompatibility : INameCompatibility
{
    public Dictionary<Type, string> TableNames => new()
    {
        [typeof(HireBookingRecord)] = "HireBookingRecord"
    };

    public Dictionary<(Type, string), string> ColumnName => [];
}