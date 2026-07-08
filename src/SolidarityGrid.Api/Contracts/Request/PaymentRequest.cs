namespace SolidarityGrid.Api.Contracts.Request;

public sealed class PaymentRequest
{
    [Required]
    [StringLength(50)]
    public string TransactionId { get; init; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; init; }

    [Required]
    [StringLength(3)]
    public string Currency { get; init; } = string.Empty;
}
