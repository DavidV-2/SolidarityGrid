namespace SolidarityGrid.Application.Mappings;

public static class PaymentMappings
{
    public static Payment ToEntity(this CreatePaymentDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto, nameof(dto));

        return Payment.Create(
            dto.TransactionId, 
            dto.Amount, 
            dto.Currency);

    }

    public static PaymentDto ToDto(this Payment entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return new PaymentDto
        {
            TransactionId = entity.TransactionId,
            Amount = entity.Amount,
            Currency = entity.Currency,
            Status = entity.Status.ToString(),
            CreateAt = entity.CreatedAt,
            CompletedAt = entity.CompletedAt
        };
    }

}
