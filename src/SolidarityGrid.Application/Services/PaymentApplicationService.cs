
namespace SolidarityGrid.Application.Services;

public sealed class PaymentApplicationService(IPaymentRepository paymentRepository) : IPaymentApplicationService
{
    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto, CancellationToken cancellationToken)
    {
        var payment = dto.ToEntity();

        await paymentRepository.AddAsync(payment, cancellationToken);
        return payment.ToDto();
    }

    public Task<PaymentDto?> GetPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<PaymentDto>> GetPaymentsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}