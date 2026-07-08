namespace SolidarityGrid.Infrastructure.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly SolidarityGridDbContext _context;

    public PaymentRepository(SolidarityGridDbContext context) => _context = context;

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(payment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Payment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .AsNoTracking()
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);
    }
}
