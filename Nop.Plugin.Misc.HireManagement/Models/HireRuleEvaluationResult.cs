using Nop.Plugin.Misc.HireManagement.Domain;

namespace Nop.Plugin.Misc.HireManagement.Models;

public partial record HireRuleEvaluationResult
{
    public bool IsValid { get; init; }

    public string Error { get; init; }

    public HirePaymentRequirement PaymentRequirement { get; init; }

    public decimal DepositAmount { get; init; }

    public static HireRuleEvaluationResult Invalid(string error)
    {
        return new HireRuleEvaluationResult
        {
            IsValid = false,
            Error = error
        };
    }

    public static HireRuleEvaluationResult Valid(HirePaymentRequirement paymentRequirement, decimal depositAmount)
    {
        return new HireRuleEvaluationResult
        {
            IsValid = true,
            PaymentRequirement = paymentRequirement,
            DepositAmount = depositAmount
        };
    }
}
