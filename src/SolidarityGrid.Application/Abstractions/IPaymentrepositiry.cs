using System.Data;

namespace SolidarityGrid.Application.Abstractions;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Payment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Payment>> GetAllPendingAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Payment>> GetOrphanTransactionsAsync(TimeSpan heartbeatTimeout, CancellationToken cancellationToken = default);
    Task<bool> TryClaimTransactionAsync(Guid paymentId, string processingNode, CancellationToken cancellationToken = default);
}