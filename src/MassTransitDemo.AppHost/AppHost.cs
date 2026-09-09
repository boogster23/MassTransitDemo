var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("messaging")
                    .WithManagementPlugin();

var postgres = builder.AddPostgres("postgres")
                    .WithPgAdmin();

var appDb = postgres.AddDatabase("appdb");

var grafana = builder.AddContainer("grafana", "grafana/grafana")
                    .WithHttpEndpoint(port: 3000, targetPort: 3000, name: "http");

builder.AddProject<Projects.MassTransitDemo_ApiService>("apiservice")
    .WithReference(messaging)
    .WithReference(appDb)
    .WaitFor(messaging);

builder.AddProject<Projects.MassTransitDemo_Worker>("worker")
    .WithReference(messaging)
    .WithReference(appDb)
    .WaitFor(messaging);

builder.Build().Run();
