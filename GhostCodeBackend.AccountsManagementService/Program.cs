
using GhostCodeBackend.AccountsManagementService.Repositories;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBakend.AccountsManagementService.Services;
using GhostCodeBakend.AccountsManagementService.Utils;
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



builder.Services.AddSingleton<IHasher, Hasher>();
builder.Services.AddSingleton<IRandomWordGenerator, RandomWordGenerator>();

builder.Services.AddSingleton<IAccountsRepository, AccountsRepository>();

builder.Services.AddScoped<IAccountsService,  AccountsService>();

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

app.MapPost("/register", async (RegisterRequestDTO req, IAccountsService accounts) =>
{
    var result = await accounts.RegisterAsync(req);
    
    return result.Item1 ? Results.Ok(new
    {
        recoveryCode =  result.Item3,
    }) : Results.InternalServerError("Account registration failed");
});

app.MapPost("/login", async (LoginRequestDTO req, IAccountsService accounts) =>
{
    var results = await accounts.LoginAsync(req);
    
    return results.Item1 ? Results.Ok(new
    {
        data = results.Item1
    }
    ) :  Results.InternalServerError("Login failed");
});

app.MapPost("/recovery", async (AccountRecoveryRequestDTO req, IAccountsService accounts) =>
{
    var results = await accounts.PasswordReset(req.Login, req.RecoveryCode, req.NewPassword);
    
    return results.Item1 ? Results.Ok(new
    {
        newRecovery = results.Item2,
    }) : Results.InternalServerError("Password reset failed");
});




app.Run();
