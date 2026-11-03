using System;
using Nop.Plugin.Misc.HireManagement.Domain;
using Nop.Plugin.Misc.HireManagement.Models;

namespace Nop.Plugin.Misc.HireManagement.Services;

public partial class HireRulesService : IHireRulesService
{
    private const int FullPaymentThresholdDays = 28;
    private readonly HireManagementSettings _hireManagementSettings;

    public HireRulesService(HireManagementSettings hireManagementSettings)
    {
        _hireManagementSettings = hireManagementSettings;
    }

    public HireRuleEvaluationResult Evaluate(HireRequest request, DateTime nowUtc)
    {
        if (request.StartUtc <= nowUtc)
            return HireRuleEvaluationResult.Invalid("Start date and time must be in the future.");

        if (request.EndUtc <= request.StartUtc)
            return HireRuleEvaluationResult.Invalid("End date and time must be later than start date and time.");

        var daysUntilStart = (request.StartUtc - nowUtc).TotalDays;
        var paymentRequirement = daysUntilStart > FullPaymentThresholdDays
            ? HirePaymentRequirement.FullOrDeposit
            : HirePaymentRequirement.FullOnly;

        var depositAmount = paymentRequirement == HirePaymentRequirement.FullOrDeposit
            ? request.OneDayHirePrice
            : 0m;

        return HireRuleEvaluationResult.Valid(paymentRequirement, depositAmount);
    }

    public (DateTime BlockFromUtc, DateTime BlockToUtc) GetOperationalBlockWindow(DateTime rentalStartUtc, DateTime rentalEndUtc)
    {
        var blockedFrom = rentalStartUtc.AddHours(-_hireManagementSettings.BufferHoursBefore);
        var blockedTo = rentalEndUtc.AddHours(_hireManagementSettings.BufferHoursAfter);

        return (blockedFrom, blockedTo);
    }
}
