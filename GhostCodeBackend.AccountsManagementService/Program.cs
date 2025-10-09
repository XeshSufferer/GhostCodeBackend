
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddAspNetCoreInstrumentation())
    .WithMetrics(m => m.AddAspNetCoreInstrumentation())
    .UseOtlpExporter();

builder.WebHost.ConfigureKestrel(o => o.ListenAnyIP(8333));
builder.AddServiceDefaults();
var app = builder.Build();


app.MapDefaultEndpoints(); 
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapGet("/{word}", (string word) => Results.Ok(word));

app.Run();
