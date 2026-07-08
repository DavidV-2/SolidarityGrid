namespace SolidarityGrid.Api.Contracts.Responses;

public sealed class PaymentResponse
{
    public string TransactionId { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Currency { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;
}
