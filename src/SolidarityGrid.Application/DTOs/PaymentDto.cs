namespace SolidarityGrid.Application.DTOs;

public sealed class PaymentDto
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreateAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}
