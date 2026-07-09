using System.Net.NetworkInformation;

namespace SolidarityGrid.Domain.Entities;

public sealed class Payment
{
    private Payment(string transactionId, decimal amount, string currency)
    {
        Id = Guid.NewGuid();
        TransactionId = transactionId;
        Amount = amount;
        Currency = currency;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }


    public Guid Id { get; private set; }
    public string TransactionId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? ProcessingNode { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? LastHeartbeatUtc { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public byte[] Version { get; private set; } = default;


    public static Payment Create(string transactionId, decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentOutOfRangeException("TransactionId is required", nameof(transactionId));
        if (amount <= 0)
            throw new ArgumentOutOfRangeException("Amount must be greater than 0", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentOutOfRangeException("Currency is required", nameof(currency));

        return new Payment(transactionId, amount, currency);
    }
    public void StartProcessing(string processingNode)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot start processing a payment with status {Status}");
        
        ProcessingNode = processingNode;
        Status = PaymentStatus.Processing;
        LastHeartbeatUtc = DateTimeOffset.UtcNow;
    }
    public void Complete()
    {
        if(Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Cannot complete a payment with status {Status}");

        Status = PaymentStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        ProcessingNode = null;
        LastHeartbeatUtc = null;
    }
    public void Fail()
    {
        if(Status != PaymentStatus.Processing) 
            throw new InvalidOperationException($"Cannot fail a payment with status {Status}");

        Status = PaymentStatus.Failed;
        CompletedAt = DateTimeOffset.UtcNow;
        ProcessingNode = null;
        LastHeartbeatUtc = null;
    }
    public void RenewLease()
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Cannot renew lease for payment with status {Status}");

        LastHeartbeatUtc = DateTimeOffset.UtcNow;
    }
}
