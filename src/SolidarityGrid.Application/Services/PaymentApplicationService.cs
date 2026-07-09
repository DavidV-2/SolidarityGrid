
using SolidarityGrid.Domain.Enums;

namespace SolidarityGrid.Application.Services;

public sealed class PaymentApplicationService : IPaymentApplicationService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentApplicationService(IPaymentRepository paymentRepository, IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }


    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto, CancellationToken cancellationToken)
    {
        var existing = await _paymentRepository.GetByTransactionIdAsync(dto.TransactionId, cancellationToken);
        if (existing != null)
        {
            return existing.ToDto();
        }

        var payment = dto.ToEntity();
        await _paymentRepository.AddAsync(payment, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return payment.ToDto();
    }

    public async Task<PaymentDto?> GetPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByTransactionIdAsync(transactionId, cancellationToken);
        return payment?.ToDto();
    }

    public async Task<PaymentDto> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        return payment?.ToDto();
    }

    public async Task<IReadOnlyCollection<PaymentDto>> GetPaymentsAsync(CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetAllAsync(cancellationToken);
        return payments.Select(p => p.ToDto()).ToList();
    }
}