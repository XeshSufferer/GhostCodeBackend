using Aspire.Hosting.Yarp.Transforms;

var builder = DistributedApplication.CreateBuilder(args);



var minio = builder.AddMinioContainer("minio");
var cache = builder.AddRedis("cache");
var mongo = builder.AddMongoDB("mongodb", 3363)
    .AddDatabase("mainMongo", "main");



var gateway = builder.AddYarp("gateway").WithHostPort(8080);
var rabbitmq = builder.AddRabbitMQ("rabbitmq");

var accountsManagementService =
    builder.AddDockerfile("account-management", "..", "GhostCodeBackend.AccountsManagementService/Dockerfile")
        .WithHttpEndpoint(0000, 8333, "accounts-http")
        .WithEnvironment("ASPNETCORE_HTTP_PORTS", "8333")
        .WaitFor(mongo)
        .WaitFor(cache)
        .WithReference(mongo, "mongodb")
        .WithReference(cache, "redis")
        .WithReference(rabbitmq, "rabbitmq")
        .WithOtlpExporter()
        .WithImageTag("dev");;

var tokenFactory =
    builder.AddDockerfile("token-factory", "..", "GhostCodeBackend.TokenFactory/Dockerfile")
        .WithHttpEndpoint(0000, 8222, "token-factory")
        .WithEnvironment("ASPNETCORE_HTTP_PORTS", "8222")
        .WaitFor(mongo)
        .WaitFor(cache)
        .WithReference(mongo, "mongodb")
        .WithReference(cache, "redis")
        .WithReference(rabbitmq, "rabbitmq")
        .WithEnvironment("JWTAudience", "Audience")
        .WithEnvironment("JWTIssuer", "Issuer")
        .WithEnvironment("JWTKey", "SUPER_SECRET_256_BIT_KEY_AT_LEAST_32_CHARS")
        .WithEnvironment("JWTExpireMinutes", "15")
        .WithEnvironment("RefreshExpireDays", "30")
        .WithOtlpExporter()
        .WithImageTag("dev");;

var userContent =
    builder.AddDockerfile("user-content", "..", "GhostCodeBackend.UserContentService/Dockerfile")
        .WithHttpEndpoint(0000, 8444, "user-content")
        .WithEnvironment("ASPNETCORE_HTTP_PORTS", "8444")
        .WaitFor(mongo)
        .WaitFor(cache)
        .WaitFor(minio)
        .WithReference(mongo, "mongodb")
        .WithReference(cache, "redis")
        .WithReference(minio, "minio")
        .WithEnvironment("JWTAudience", "Audience")
        .WithEnvironment("JWTIssuer", "Issuer")
        .WithEnvironment("JWTKey", "SUPER_SECRET_256_BIT_KEY_AT_LEAST_32_CHARS")
        .WithEnvironment("JWTExpireMinutes", "15")
        .WithEnvironment("RefreshExpireDays", "30")
        .WithOtlpExporter()
        .WithImageTag("dev");;

var postManagemet = 
    builder.AddDockerfile("post-management", "..", "GhostCodeBackend.PostManagement/Dockerfile")
        .WithHttpEndpoint(0000, 8555, "post-management")
        .WithEnvironment("ASPNETCORE_HTTP_PORTS", "8555")
        .WaitFor(mongo)
        .WaitFor(cache)
        .WithReference(mongo, "mongodb")
        .WithReference(cache, "redis")
        .WithEnvironment("JWTAudience", "Audience")
        .WithEnvironment("JWTIssuer", "Issuer")
        .WithEnvironment("JWTKey", "SUPER_SECRET_256_BIT_KEY_AT_LEAST_32_CHARS")
        .WithEnvironment("JWTExpireMinutes", "15")
        .WithEnvironment("RefreshExpireDays", "30")
        .WithOtlpExporter()
        .WithImageTag("dev");;



gateway.WithConfiguration(yarp =>
{
    yarp.AddRoute("/api/accounts/{**catch-all}", accountsManagementService.GetEndpoint("accounts-http"))
        .WithTransformPathRemovePrefix("/api/accounts");
    
    yarp.AddRoute("/api/tokens/{**catch-all}", tokenFactory.GetEndpoint("token-factory"))
        .WithTransformPathRemovePrefix("/api/tokens");
    
    yarp.AddRoute("/api/content/{**catch-all}", userContent.GetEndpoint("user-content"))
        .WithTransformPathRemovePrefix("/api/content");
    
    yarp.AddRoute("/api/posts/{**catch-all}", postManagemet.GetEndpoint("post-management"))
        .WithTransformPathRemovePrefix("/api/posts");
})
.WaitFor(tokenFactory)
.WaitFor(accountsManagementService)
.WaitFor(userContent);



builder.Build().Run();