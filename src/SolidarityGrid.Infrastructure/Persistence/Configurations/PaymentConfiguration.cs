namespace SolidarityGrid.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.TransactionId);

        builder.Property(p => p.TransactionId)
                .HasMaxLength(100)
                .IsRequired();
        builder.Property(p => p.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

        builder.Property(p => p.Currency)
                 .HasMaxLength(3)
                 .IsRequired();

        builder.Property(x => x.Status)
                 .HasConversion<string>()
                 .HasMaxLength(20)
                 .IsRequired();

        builder.Property(x => x.ProcessingNode)
                .HasMaxLength(100);

        builder.Property(x => x.CreatedAt)
                .IsRequired();

        builder.Property(x => x.CompletedAt)
                .IsRequired(false);
    }
}
