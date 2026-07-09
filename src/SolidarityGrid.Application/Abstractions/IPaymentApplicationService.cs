namespace SolidarityGrid.Application.Abstractions;

public interface IPaymentApplicationService
{
    Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto payment, CancellationToken cancellationToken = default);

    Task<PaymentDto?> GetPaymentAsync(string transactionId,CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PaymentDto>> GetPaymentsAsync(CancellationToken cancellationToken = default);
    Task<PaymentDto> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
