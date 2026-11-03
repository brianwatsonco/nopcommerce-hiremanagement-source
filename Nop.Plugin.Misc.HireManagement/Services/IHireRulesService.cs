using System;
using Nop.Plugin.Misc.HireManagement.Models;

namespace Nop.Plugin.Misc.HireManagement.Services;

public partial interface IHireRulesService
{
    HireRuleEvaluationResult Evaluate(HireRequest request, DateTime nowUtc);

    (DateTime BlockFromUtc, DateTime BlockToUtc) GetOperationalBlockWindow(DateTime rentalStartUtc, DateTime rentalEndUtc);
}
