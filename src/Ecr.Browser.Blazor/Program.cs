using System.Text.Json;
using Amazon.ECR;
using Ecr.Browser;
using Ecr.Browser.Blazor;
using MudBlazor.Services;
using Ecr.Browser.Blazor.Components;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisDistributedCache("cache");
builder.Services.AddFusionCache()
    .WithSerializer(_ =>
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.TypeInfoResolverChain.Add(OurJsonContext.Default);
            return new FusionCacheSystemTextJsonSerializer(jsonSerializerOptions);
        })
    .WithRegisteredDistributedCache();

// Add MudBlazor services
builder.Services.AddMudServices();
builder.Services.AddOpenTelemetry()
    .WithTracing(b => b.AddFusionCacheInstrumentation(o =>
    {
        o.IncludeMemoryLevel = true;
    })).WithMetrics(b =>
    {
        b.AddFusionCacheInstrumentation(o =>
        {
            o.IncludeMemoryLevel = true;
            o.IncludeDistributedLevel = true;
        });
    });

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAWSService<IAmazonECR>();
builder.Services.AddSingleton<EcrClient>();
builder.Services.AddSingleton<EcrService>();
builder.Services.AddScoped<ClipboardService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
