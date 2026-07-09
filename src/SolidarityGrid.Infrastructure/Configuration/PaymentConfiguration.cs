

namespace SolidarityGrid.Infrastructure.Configuration;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.HasIndex(p => p.TransactionId)
               .HasDatabaseName("IX_Payments_TransactionId")
               .IsUnique();

        builder.Property(p => p.TransactionId)
                .HasMaxLength(100)
                .IsRequired();

        builder.Property(p => p.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

        builder.Property(p => p.Currency)
                 .HasMaxLength(3)
                 .IsFixedLength()
                 .IsRequired();

        builder.Property(x => x.Status)
                 .HasConversion<int>()
                 .IsRequired();

        builder.Property(x => x.ProcessingNode)
                .HasMaxLength(100);

        builder.Property(p => p.LastHeartbeatUtc)
                .IsRequired(false);

        builder.Property(x => x.CreatedAt)
                .IsRequired();

        builder.Property(x => x.CompletedAt)
                .IsRequired(false);

        builder.Property(p => p.Version)
               .IsRowVersion();
    }
}
