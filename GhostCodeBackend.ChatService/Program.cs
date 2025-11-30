using Cassandra;
using Cassandra.Mapping;
using GhostCodeBackend.ChatService.Hubs;
using GhostCodeBackend.ChatService.Repositories;
using GhostCodeBackend.ChatService.Services;
using GhostCodeBackend.Shared.Models;
using GhostCodeBackend.Shared.Сache;
using Microsoft.AspNetCore.Session;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.AddRedisDistributedCache("redis");

builder.AddDefaultAuthorization(builder.Configuration);
builder.AddDefaultCors();
builder.AddServiceDefaults();


builder.Services.AddScoped<ICacheService, RedisService>();
builder.Services.AddSingleton<IChatsRepository, ChatsRepository>();
builder.Services.AddSingleton<IMessagesRepository, MessagesRepository>();
builder.Services.AddScoped<ICacheService, RedisService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IMessagingService, MessagingService>();

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(builder.Configuration.GetConnectionString("mongodb")));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("main");
});

builder.Services.AddSingleton<IMongoCollection<Chat>>(sp =>
{
    var db = sp.GetRequiredService<IMongoDatabase>();
    return db.GetCollection<Chat>("chats_metadata");
});

builder.Services.AddSingleton<Cassandra.ISession>(p =>
{
    var cluster = Cluster.Builder()
        .AddContactPoint(builder.Configuration["scylla__host"])
        .WithPort(int.Parse(builder.Configuration["scylla__port"]))
        .Build();
    
    return cluster.Connect("messages");
});

builder.Services.AddSingleton<IMapper, Mapper>(p =>
{
    var session = p.GetRequiredService<Cassandra.ISession>();
    
    return new Mapper(session);
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseDefaultAuth();
app.UseDefaultCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHub<ChatHub>("/chat");

app.Run();
