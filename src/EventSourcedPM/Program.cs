using System;
using EventSourcedPM;
using EventSourcedPM.Configuration;
using EventSourcedPM.Configurators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

var settings = SettingsResolver.GetSettings();

// Proper logger in not available at this point
Console.WriteLine(settings.ToString());

if (!InfrastructureWaitPolicy.WaitForInfrastructure(settings))
{
    throw new InvalidOperationException("Infrastructure service(s) not available");
}

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(settings.ApiBaseUrl);
builder
    .Services.AddApplicationSerilog()
    .AddApplicationMessageBus(settings)
    .AddApplicationEventStore(settings)
    .AddApplicationProcessManager();

var app = builder.Build();
app.AddApiEndpoints();

app.Run();
