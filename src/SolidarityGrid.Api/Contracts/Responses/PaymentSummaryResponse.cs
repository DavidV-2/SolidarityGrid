namespace SolidarityGrid.Api.Contracts.Responses;

public sealed class PaymentSummaryResponse
{
    public string TransactionId { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Status { get; init; } = string.Empty;
}
