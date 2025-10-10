using Aspire.Hosting.Yarp.Transforms;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
var mongo = builder.AddMongoDB("mongodb", 3363)
    .AddDatabase("mainMongo", "main");



var gateway = builder.AddYarp("gateway");


var accountsManagementService =
    builder.AddDockerfile("account-management", "..", "GhostCodeBackend.AccountsManagementService/Dockerfile")
        .WithHttpEndpoint(0000, 8333, "accounts-http")
        .WithEnvironment("ASPNETCORE_HTTP_PORTS", "8333")
        .WaitFor(mongo)
        .WaitFor(cache)
        .WithReference(mongo, "mongodb")
        .WithReference(cache, "redis");




gateway.WithConfiguration(yarp =>
{
    yarp.AddRoute("/api/accounts/{**catch-all}", accountsManagementService.GetEndpoint("accounts-http"))
        .WithTransformPathRemovePrefix("/api/accounts");
});



builder.Build().Run();