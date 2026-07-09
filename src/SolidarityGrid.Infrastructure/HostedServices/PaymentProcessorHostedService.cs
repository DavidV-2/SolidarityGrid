namespace SolidarityGrid.Infrastructure.HostedServices;

public class PaymentProcessorHostedService : BackgroundService
{
    private readonly NodeConfig _nodeConfig;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentProcessorHostedService> _logger;

    public PaymentProcessorHostedService(IServiceScopeFactory scopeFactory, ILogger<PaymentProcessorHostedService> logger, IOptions<NodeConfig> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _nodeConfig = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payment Processor Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPaymentsAsync(stoppingToken);
                await Task.Delay(
                    TimeSpan.FromSeconds(_nodeConfig.ProcessingIntervalSeconds), stoppingToken);
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
        var nodeName = _nodeConfig.NodeName;

        var pendingTransactions = await repository.GetAllPendingAsync(cancellationToken);

        foreach (var payment in pendingTransactions)
        {
            await ProcessClaimedPaymentAsync(payment.Id, nodeName, repository, unitOfWork, cancellationToken);
        }


        var orphanTransactions =
            await repository.GetOrphanTransactionsAsync(
                TimeSpan.FromSeconds(_nodeConfig.HeartbeatTimeoutSeconds),
                cancellationToken);

        foreach (var transaction in orphanTransactions)
        {
            _logger.LogWarning("Transaction {TransactionId} is orphaned. Last heartbeat at {LastHeartbeat}. Attempting to reclaim...",
                               transaction.TransactionId,
                               transaction.LastHeartbeatUtc);

            await ProcessClaimedPaymentAsync(transaction.Id, nodeName, repository, unitOfWork, cancellationToken);
        }

    }
    private async Task ProcessWithHeartbeatAsync(Payment payment, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        var processingTime = Random.Shared.Next(5000, 10000);
        var elapsed = 0;
        var heartbeatInterval = TimeSpan.FromSeconds(_nodeConfig.HeartbeatIntervalSeconds);


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

            var remaining = processingTime - elapsed;

            await Task.Delay(
                remaining < heartbeatInterval.TotalMilliseconds
                    ? remaining
                    : (int)heartbeatInterval.TotalMilliseconds,
                cancellationToken);

            elapsed += Math.Min(
                remaining,
                (int)heartbeatInterval.TotalMilliseconds);
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
    private async Task ProcessClaimedPaymentAsync(Guid paymentId, string nodeName, IPaymentRepository repository, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        var claimed = await repository.TryClaimTransactionAsync(paymentId, nodeName, cancellationToken);

        if (!claimed)
            return;

        var payment = await repository.GetByIdAsync(
            paymentId,
            cancellationToken);

        if (payment is null)
            return;

        _logger.LogInformation(
            "Node {Node} Processing transaction {TransactionId}",
            nodeName,
            payment.TransactionId);

        await ProcessWithHeartbeatAsync(
            payment,
            unitOfWork,
            cancellationToken);
    }
}
