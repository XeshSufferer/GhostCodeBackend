using Aspire.Hosting.Yarp.Transforms;
using GhostCodeBackend.AppHost;

var builder = DistributedApplication.CreateBuilder(args);



var minio = builder.AddMinioContainer("minio")
    .WithImageTag("latest")
    .WithContainerName("minio");
var cache = builder.AddRedis("cache")
    .WithImageTag("latest")
    .WithContainerName("redis-cache");
var mongo = builder.AddMongoDB("mongodb", 3363)
    .WithContainerName("mongodb")
    .AddDatabase("mainMongo", "main");


var gitea = builder.AddContainer("gitea", "gitea/gitea:1.22-rootless")
    .WithHttpEndpoint(3000, 3000, "gitea-http")
    .WithEnvironment("GITEA__service__DISABLE_REGISTRATION", "true")
    .WithEnvironment("GITEA__security__DISABLE_PASSWORD_CHANGE", "true")
    .WithEnvironment("GITEA__server__ROOT_URL",
        "http://localhost:3000/")
    .WithVolume("/repos")
    .WithImageTag("dev")
    .WithContainerName("gitea");


var gateway = builder.AddYarp("gateway")
    .WithHostPort(8080)
    .WithContainerName("gateway");
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithContainerName("rabbitmq");

var scylla = builder.AddContainer("scylla", "scylladb/scylla")
    .WithImageTag("latest")
    .WithContainerName("scylladb")
    .WithEnvironment("SCYLLA_PORT", "8000")
    .WithEnvironment("LISTEN_ADDR", "0.0.0.0");

var accountsManagementService =
    builder.AddDockerfile("account-management", "..", "GhostCodeBackend.AccountsManagementService/Dockerfile")
        .WithHttpEndpoint(0000, 8333, "accounts-http")
        .WithPort(8333)
        .WaitFor(mongo)
        .WaitFor(cache)
        .WithReference(mongo, "mongodb")
        .WithReference(cache, "redis")
        .WithReference(rabbitmq, "rabbitmq")
        .WithOtlpExporter()
        .WithContainerName("account-management")
        .WithImageTag("dev");

var tokenFactory =
    builder.AddDockerfile("token-factory", "..", "GhostCodeBackend.TokenFactory/Dockerfile")
        .WithHttpEndpoint(0000, 8222, "token-factory")
        .WithPort(8222)
        .WaitFor(mongo)
        .WaitFor(cache)
        .WithReference(mongo, "mongodb")
        .WithReference(cache, "redis")
        .WithReference(rabbitmq, "rabbitmq")
        .WithJwtAuthSettings()
        .WithOtlpExporter()
        .WithContainerName("token-factory")
        .WithImageTag("dev");

var userContent =
    builder.AddDockerfile("user-content", "..", "GhostCodeBackend.UserContentService/Dockerfile")
        .WithHttpEndpoint(0000, 8444, "user-content")
        .WithPort(8444)
        .WaitFor(mongo)
        .WaitFor(cache)
        .WaitFor(minio)
        .WithReference(mongo, "mongodb")
        .WithReference(cache, "redis")
        .WithReference(minio, "minio")
        .WithJwtAuthSettings()
        .WithOtlpExporter()
        .WithContainerName("user-content")
        .WithImageTag("dev");

var postManagemet = 
    builder.AddDockerfile("post-management", "..", "GhostCodeBackend.PostManagement/Dockerfile")
        .WithHttpEndpoint(0000, 8555, "post-management")
        .WithPort(8555)
        .WaitFor(mongo)
        .WaitFor(cache)
        .WithReference(mongo, "mongodb")
        .WithReference(cache, "redis")
        .WithJwtAuthSettings()
        .WithOtlpExporter()
        .WithContainerName("post-management")
        .WithImageTag("dev");

var gitUsage = builder.AddDockerfile("git-usage-management", "..", "GhostCodeBackend.GitUsageService/Dockerfile")
    .WithHttpEndpoint(0000, 8888, "git-usage-management")
    .WithPort(8888)
    .WithEnvironment("AdminPassword", "just_try_it")
    .WithEnvironment("AdminLogin", "just_try_it")
    .WithEnvironment("AdminMail", "just_try_it@mail.ru")
    .WithEnvironment("Gitea:ApiUrl", "http://gitea:3000/api/v1")
    .WithReference(rabbitmq)
    .WaitFor(gitea)
    .WaitFor(rabbitmq)
    .WithContainerName("git-usage")
    .WithImageTag("dev");

var notifications = 
    builder.AddDockerfile("notifications", "..", "GhostCodeBackend.NotificationService/Dockerfile")
        .WithPort(8111)
        .WaitFor(rabbitmq)
        .WaitFor(cache)
        .WithReference(rabbitmq, "rabbitmq")
        .WithReference(cache, "redis")
        .WithJwtAuthSettings()
        .WithOtlpExporter()
        .WithContainerName("notifications")
        .WithImageTag("dev");

var chats = builder.AddDockerfile("chats", "..", "GhostCodeBackend.ChatService/Dockerfile")
    .WithPort(8666)
    .WaitFor(cache)
    .WaitFor(scylla)
    .WithReference(cache, "redis")
    .WithJwtAuthSettings()
    .WithOtlpExporter()
    .WithEnvironment("scylla__port", "8000")
    .WithEnvironment("scylla__host", "scylladb")
    .WithContainerName("chats")
    .WithImageTag("dev");

gateway.WithReference(accountsManagementService.GetEndpoint("account-management"));

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