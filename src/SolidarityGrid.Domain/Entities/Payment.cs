namespace SolidarityGrid.Domain.Entities;

public sealed class Payment
{
    private Payment(string transactionId, decimal amount, string currency)
    {
        TransactionId = transactionId;
        Amount = amount;
        Currency = currency;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string TransactionId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public PaymentStatus Status { get;  private set; }
    public string? ProcessingNode { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? CompletedAt { get; private set; }


    public static Payment Create(string transactionId, decimal amount, string currency) => new Payment(transactionId, amount, currency);
    public void StartProcessing(string processingNode)
    {
        if (Status == PaymentStatus.Completed)
            return;

        Status = PaymentStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }
    public void Complete()
    {
        if (Status == PaymentStatus.Completed)
            return;

        if(Status != PaymentStatus.Processing)
            return;

        Status = PaymentStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        ProcessingNode = null;
    }
    public void Fail()
    {
        if(Status != PaymentStatus.Processing) 
            return;
        Status = PaymentStatus.Failed;
        ProcessingNode = null;
    }
}
