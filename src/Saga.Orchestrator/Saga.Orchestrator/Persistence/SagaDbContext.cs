using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using Saga.Orchestrator.OrderManager;
using Shared.DTOs.Basket;
using Shared.DTOs.Order;

public class SagaDbContext : DbContext
{
    public SagaDbContext(DbContextOptions<SagaDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrderSagaState> SagaStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new OrderSagaStateConfiguration());
    }
}

public class OrderSagaStateConfiguration : IEntityTypeConfiguration<OrderSagaState>
{
    public void Configure(EntityTypeBuilder<OrderSagaState> builder)
    {
        builder.ToTable("OrderSagaState");

        builder.HasKey(x => x.CorrelationId);
        builder.Property(x => x.CorrelationId).IsRequired();
        builder.Property(x => x.CurrentState).IsRequired();

        // Các cột có thể NULL
        builder.Property(x => x.UserName).IsRequired(false); // string là kiểu tham chiếu, có thể NULL
        builder.Property(x => x.InventoryDocumentNo).IsRequired(false); // string, có thể NULL
        builder.Property(x => x.Order).IsRequired(false); 
        builder.Property(x => x.Cart).IsRequired(false); 

        // Serialize complex objects (Cart and Order) as JSON, có thể NULL
        builder.Property(x => x.Cart)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => v == null ? null : JsonSerializer.Deserialize<CartDto>(v, (JsonSerializerOptions)null))
            .IsRequired(false);

        builder.Property(x => x.Order)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => v == null ? null : JsonSerializer.Deserialize<OrderDto>(v, (JsonSerializerOptions)null))
            .IsRequired(false);
    }
}