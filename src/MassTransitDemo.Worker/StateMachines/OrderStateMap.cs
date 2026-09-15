using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MassTransitDemo.Worker.StateMachines;

public class OrderStateMap : SagaClassMap<OrderState>
{
    protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
    {
        entity.ToTable("OrderStates");

        entity.Property(x => x.CurrentState).HasMaxLength(64);
        entity.Property(x => x.CustomerNumber).HasMaxLength(128);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.PaymentTransactionId).HasMaxLength(64);
        entity.Property(x => x.FailureReason).HasMaxLength(256);

        entity.Property(x => x.RowVersion).IsRowVersion();
    }
}