namespace SolidarityGrid.Api.Contracts.Responses;

public sealed class PaymentResponse
{
    public Guid Id { get; init; }
    public string TransactionId { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Currency { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;
    public string? ProcessingNode { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}
