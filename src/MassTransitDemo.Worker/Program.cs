using MassTransit;
using MassTransitDemo.Worker.Consumers;
using MassTransitDemo.Worker.StateMachines;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SubmitOrderConsumer>();
    x.AddConsumer<ProcessPaymentConsumer>();

    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Optimistic;
            r.AddDbContext<DbContext, OrderSagaDbContext>((provider, options) =>
            {
                var connString = builder.Configuration.GetConnectionString("appdb");
                options.UseNpgsql(connString);
            });
        });

    x.UsingRabbitMq((context, cfg) =>
    {
        var connString = builder.Configuration.GetConnectionString("messaging");
        cfg.Host(connString);

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
