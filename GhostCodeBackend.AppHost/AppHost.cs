using Aspire.Hosting.Yarp.Transforms;

var builder = DistributedApplication.CreateBuilder(args);



var minio = builder.AddMinioContainer("minio");
var cache = builder.AddRedis("cache");
var mongo = builder.AddMongoDB("mongodb", 3363)
    .AddDatabase("mainMongo", "main");


var gitea = builder.AddContainer("gitea", "gitea/gitea:1.22-rootless")
    .WithHttpEndpoint(3000, 3000, "gitea-http")
    .WithEnvironment("GITEA__service__DISABLE_REGISTRATION", "true")
    .WithEnvironment("GITEA__security__DISABLE_PASSWORD_CHANGE", "true")
    .WithEnvironment("GITEA__server__ROOT_URL",
        "http://localhost:3000/")
    .WithVolume("/repos");


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
        .WithImageTag("dev");

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
        .WithImageTag("dev");

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
        .WithImageTag("dev");

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
        .WithImageTag("dev");

var gitUsage = builder.AddDockerfile("git-usage-management", "..", "GhostCodeBackend.GitUsageService/Dockerfile")
    .WithHttpEndpoint(0000, 8888, "git-usage-management")
    .WithEnvironment("ASPNETCORE_HTTP_PORTS", "8888")
    .WithEnvironment("AdminPassword", "just_try_it")
    .WithEnvironment("AdminLogin", "just_try_it")
    .WithEnvironment("AdminMail", "just_try_it@mail.ru")
    .WithEnvironment("Gitea:ApiUrl", "http://gitea:3000/api/v1")
    .WithReference(rabbitmq)
    .WaitFor(gitea)
    .WaitFor(rabbitmq)
    .WithImageTag("dev");

var notifications = 
    builder.AddDockerfile("notifications", "..", "GhostCodeBackend.NotificationService/Dockerfile")
        .WithEnvironment("ASPNETCORE_HTTP_PORTS", "8111")
        .WaitFor(rabbitmq)
        .WaitFor(cache)
        .WithReference(rabbitmq, "rabbitmq")
        .WithReference(cache, "redis")
        .WithEnvironment("JWTAudience", "Audience")
        .WithEnvironment("JWTIssuer", "Issuer")
        .WithEnvironment("JWTKey", "SUPER_SECRET_256_BIT_KEY_AT_LEAST_32_CHARS")
        .WithEnvironment("JWTExpireMinutes", "15")
        .WithEnvironment("RefreshExpireDays", "30")
        .WithOtlpExporter()
        .WithImageTag("dev");

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