namespace SolidarityGrid.Infrastructure.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly SolidarityGridDbContext _context;

    public PaymentRepository(SolidarityGridDbContext context) => _context = context;

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default) => await _context.Payments.AddAsync(payment, cancellationToken);

    public async Task<IReadOnlyCollection<Payment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .AsNoTracking()
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default) => await _context.Payments
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);

    public async Task<IReadOnlyCollection<Payment>> GetAllPendingAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.Status == PaymentStatus.Pending)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Payment>> GetOrphanTransactionsAsync(TimeSpan heartbeatTimeout, CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTimeOffset.UtcNow.Subtract(heartbeatTimeout);

        return await _context.Payments
            .Where(p => p.Status == PaymentStatus.Processing
                        && p.LastHeartbeatUtc != null
                        && p.LastHeartbeatUtc < cutoffTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> TryClaimTransactionAsync(Guid paymentId, string ProcessingNode, CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(
                p => p.Id ==  paymentId && 
                    p.Status == PaymentStatus.Pending,
                cancellationToken);

        if (payment is null)
            return false;

        try
        {
            payment.StartProcessing(ProcessingNode);

            // Atomic claim.
            // Save immediately so no other node can acquire the payment.
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }

    }
}
