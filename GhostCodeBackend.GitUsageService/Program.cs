using GhostCodeBackend.GitUsageService.Adapters;
using GhostCodeBackend.GitUsageService.Helpers;
using GhostCodeBackend.GitUsageService.RPC;
using GhostCodeBackend.GitUsageService.Services;
using GhostCodeBackend.Shared.RPC.MessageBroker;
using Gitea.Net.Api;
using Gitea.Net.Client;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

var cfg =  builder.Configuration;

builder.AddRabbitMQClient("rabbitmq");

builder.Services.AddSingleton<IAdminApi, AdminApi>(_ => new AdminApi(new Configuration()
{
    BasePath = cfg["Gitea:ApiUrl"],         
    Password   = cfg["AdminPassword"],
    Username = cfg["AdminLogin"],
    
}));

Console.WriteLine($"Gitea API URL: {cfg["Gitea:ApiUrl"]}");
Console.WriteLine($"Admin Login: {cfg["AdminLogin"]}");
Console.WriteLine($"Admin Password: {cfg["AdminPassword"]}");

builder.Services.AddScoped<IRandomWordHelper, RandomWordHelper>();
builder.Services.AddScoped<IGitUsageService, GitUsageService>();
builder.Services.AddScoped<IGiteaAdapter, GiteaAdapter>();
builder.Services.AddScoped<IRabbitMQService, RabbitMQService>();
builder.Services.AddSingleton<IRpcConsumer, RpcConsumer>();

var app = builder.Build();
app.MapDefaultEndpoints();




if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


await app.Services.GetRequiredService<IRpcConsumer>().StartConsumingAsync();

app.Run();
