using Aspire.Hosting.Yarp.Transforms;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
var mongo = builder.AddMongoDB("mongodb", 3363)
    .AddDatabase("mainMongo", "main");



var gateway = builder.AddYarp("gateway").WithHostPort(8080);


var accountsManagementService =
    builder.AddDockerfile("account-management", "..", "GhostCodeBackend.AccountsManagementService/Dockerfile")
        .WithHttpEndpoint(0000, 8333, "accounts-http")
        .WithEnvironment("ASPNETCORE_HTTP_PORTS", "8333")
        .WaitFor(mongo)
        .WaitFor(cache)
        .WithReference(mongo, "mongodb")
        .WithReference(cache, "redis");

var tokenFactory =
    builder.AddDockerfile("token-factory", "..", "GhostCodeBackend.TokenFactory/Dockerfile")
        .WithHttpEndpoint(0000, 8222, "token-factory")
        .WithEnvironment("ASPNETCORE_HTTP_PORTS", "8222")
        .WaitFor(mongo)
        .WaitFor(cache)
        .WithReference(mongo, "mongodb")
        .WithReference(cache, "redis")
        .WithEnvironment("JWTAudience", "Audience")
        .WithEnvironment("JWTIssuer", "Issuer")
        .WithEnvironment("JWTKey", "SUPER_SECRET_256_BIT_KEY_AT_LEAST_32_CHARS")
        .WithEnvironment("JWTExpireMinutes", "60")
        .WithEnvironment("RefreshExpireDays", "15");




gateway.WithConfiguration(yarp =>
{
    yarp.AddRoute("/api/accounts/{**catch-all}", accountsManagementService.GetEndpoint("accounts-http"))
        .WithTransformPathRemovePrefix("/api/accounts");
    
    yarp.AddRoute("/api/tokens/{**catch-all}", tokenFactory.GetEndpoint("token-factory"))
        .WithTransformPathRemovePrefix("/api/tokens");;
})
.WaitFor(tokenFactory)
.WaitFor(accountsManagementService);



builder.Build().Run();