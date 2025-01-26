var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddValkey("cache", 6379);
cache.WithLifetime(ContainerLifetime.Session)
    .WithDataVolume();

builder
    .AddProject<Projects.Ecr_Browser_Blazor>("blazor")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache);

builder.Build().Run();
