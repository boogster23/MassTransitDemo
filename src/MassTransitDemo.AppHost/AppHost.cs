var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddAzureServiceBus("messaging")
                    .RunAsEmulator();

var postgres = builder.AddPostgres("postgres")
                    .WithPgAdmin();

var appDb = postgres.AddDatabase("appdb");


builder.Build().Run();
