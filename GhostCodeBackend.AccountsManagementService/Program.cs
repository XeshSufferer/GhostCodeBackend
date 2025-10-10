
using MongoDB.Driver;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddAspNetCoreInstrumentation())
    .WithMetrics(m => m.AddAspNetCoreInstrumentation())
    .UseOtlpExporter();

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(builder.Configuration["mongodb:connectionString"]));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(builder.Configuration["mongodb:databaseName"]);
});


builder.AddServiceDefaults();
var app = builder.Build();
app.MapDefaultEndpoints(); 

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/{word}", (string word) => Results.Ok(word));

app.Run();
