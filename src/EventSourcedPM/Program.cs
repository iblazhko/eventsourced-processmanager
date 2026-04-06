using EventSourcedPM;
using EventSourcedPM.Configuration;
using EventSourcedPM.Configurators;

var settings = SettingsResolver.GetSettings();

// Proper logger in not available at this point
Console.WriteLine(settings.ToString());

if (InfrastructureWaitPolicy.WaitForInfrastructure(settings))
{
    Console.WriteLine($"[{TimeProvider.System.GetUtcNow():O}] Infrastructure services ready");
}
else
{
    throw new InvalidOperationException("Infrastructure services not available");
}

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(settings.ApiBaseUrl);
builder.Host.AddApplicationMessageBus(settings);

// csharpier-ignore
builder.Services
    .AddApplicationSerilog(settings)
    .AddApplicationMessageBus(settings)
    .AddApplicationEventStore(settings)
    .AddApplicationProcessManager();

var app = builder.Build();
app.AddApiEndpoints();

app.Run();
