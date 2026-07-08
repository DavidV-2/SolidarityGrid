namespace SolidarityGrid.Application.DTOs;

public sealed class CreatePaymentDto
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
}
