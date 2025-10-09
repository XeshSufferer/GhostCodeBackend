using Aspire.Hosting.Yarp.Transforms;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
var mongo = builder.AddMongoDB("mongodb", 3363);



var gateway = builder.AddYarp("gateway");


builder.AddDockerComposeEnvironment("docker");

var accountsManagementService =
    builder.AddDockerfile("account-management", "..", "GhostCodeBackend.AccountsManagementService/Dockerfile")
    .WithHttpEndpoint(0000, 8333, "accounts-http");



gateway.WithConfiguration(yarp =>
{
    yarp.AddRoute("/api/accounts/{**catch-all}", accountsManagementService.GetEndpoint("accounts-http"))
        .WithTransformPathRemovePrefix("/api/accounts");
});



builder.Build().Run();