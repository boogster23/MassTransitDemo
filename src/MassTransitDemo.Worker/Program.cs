using MassTransit;
using MassTransitDemo.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SubmitOrderConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        var connString = builder.Configuration.GetConnectionString("messaging");
        cfg.Host(connString);

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
