namespace SolidarityGrid.Infrastructure.HostedServices;

public class PaymentProcessorHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentProcessorHostedService> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _heartbeatTimeout = TimeSpan.FromSeconds(10);

    public PaymentProcessorHostedService(IServiceScopeFactory scopeFactory, ILogger<PaymentProcessorHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payment Processor Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPaymentsAsync(stoppingToken);
                await Task.Delay(_processingInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in payment processing loop");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        _logger.LogInformation("Payment Processor Service stopped.");
    }

    private async Task ProcessPaymentsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var NodeName  = Environment.GetEnvironmentVariable("NODE_ID") ?? "unknown-node";

        var pendingTransactions = await repository.GetAllPendingAsync(cancellationToken);

        foreach (var payment in pendingTransactions)
        {
            await ProcessClaimedPaymentAsync(payment.Id,NodeName ,repository,unitOfWork,cancellationToken);
        }


        var orphanTransactions = await repository.GetOrphanTransactionsAsync(_heartbeatTimeout, cancellationToken);

        foreach (var transaction in orphanTransactions)
        {
            _logger.LogWarning("Transaction {TransactionId} is orphaned. Last heartbeat at {LastHeartbeat}. Attempting to reclaim...",
                               transaction.TransactionId,
                               transaction.LastHeartbeatUtc);

            await ProcessClaimedPaymentAsync(transaction.Id, NodeName , repository, unitOfWork, cancellationToken);
        }

    }
    private async Task ProcessWithHeartbeatAsync(Payment payment, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        var processingTime = Random.Shared.Next(5000, 10000);
        var elapsed = 0;
        var heartbeatInterval = 3000;

        while (elapsed < processingTime)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Processing cancelled for transaction {TransactionId}", payment.TransactionId);
                return;
            }

            try
            {
                payment.RenewLease();
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning(
                    "Lost ownership of transaction {TransactionId}.",
                    payment.TransactionId);

                return;
            }

            var remaining = Math.Min(heartbeatInterval, processingTime - elapsed);
            await Task.Delay(remaining, cancellationToken);
            elapsed += remaining;
        }

        try
        {
            payment.Complete();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning(
                "Transaction {TransactionId} was completed by another node.",
                payment.TransactionId);
        }
    }
    private async Task ProcessClaimedPaymentAsync(Guid paymentId, string NodeName , IPaymentRepository repository, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        var claimed = await repository.TryClaimTransactionAsync(
        paymentId,
        NodeName ,
        cancellationToken);

        if (!claimed)
            return;

        var payment = await repository.GetByIdAsync(
            paymentId,
            cancellationToken);

        if (payment is null)
            return;

        _logger.LogInformation(
            "Processing transaction {TransactionId}",
            payment.TransactionId);

        await ProcessWithHeartbeatAsync(
            payment,
            unitOfWork,
            cancellationToken);
    }
}
