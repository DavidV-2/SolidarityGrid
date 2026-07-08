namespace SolidarityGrid.Application.Abstractions;

public interface IPaymentRepository
{
    Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Payment>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Payment payment,CancellationToken cancellationToken = default);

    Task UpdateAsync(Payment payment,CancellationToken cancellationToken = default);
}
