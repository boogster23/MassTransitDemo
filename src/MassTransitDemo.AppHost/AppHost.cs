var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("messaging")
                    .WithManagementPlugin();

var postgres = builder.AddPostgres("postgres")
                    .WithPgAdmin();

var appDb = postgres.AddDatabase("appdb");

builder.AddContainer("prometheus", "prom/prometheus")
        .WithBindMount("./prometheus.yml", "/etc/prometheus/prometheus.yml")
        .WithHttpEndpoint(port: 9090, targetPort: 9090, name: "http");

builder.AddContainer("grafana", "grafana/grafana")
        .WithHttpEndpoint(port: 3000, targetPort: 3000, name: "http")
        .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "admin")
        .WithEnvironment("GF_AUTH_ANONYMOUS_ENABLED", "true")
        .WithEnvironment("GF_AUTH_ANONYMOUS_ORG_ROLE", "Admin");

builder.AddProject<Projects.MassTransitDemo_ApiService>("apiservice")
    .WithReference(messaging)
    .WithReference(appDb)
    .WaitFor(messaging);

builder.AddProject<Projects.MassTransitDemo_Worker>("worker")
    .WithReference(messaging)
    .WithReference(appDb)
    .WaitFor(messaging);
builder.Build().Run();
