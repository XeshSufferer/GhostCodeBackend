using GhostCodeBackend.GitUsageService.Services;
using GhostCodeBackend.Shared.DTO.Interservice;
using GhostCodeBackend.Shared.DTO.Requests;
using GhostCodeBackend.Shared.RPC.MessageBroker;
using GhostCodeBackend.Shared.RPC.Tracker;

namespace GhostCodeBackend.GitUsageService.RPC;

public class RpcConsumer : IRpcConsumer
{
    
    private readonly IRabbitMQService _rabbit;
    private readonly IGitUsageService _gitUsage;
    private readonly ILogger<RpcConsumer> _logger;

    public RpcConsumer(IRabbitMQService rabbit, IGitUsageService gitUsage, ILogger<RpcConsumer> logger)
    {
        _rabbit = rabbit;
        _gitUsage = gitUsage;
        _logger = logger;
    }

    public async Task StartConsumingAsync()
    {
        await _rabbit.InitializeAsync();
        await _rabbit.StartConsumingAsync<Message<RegisterRequestDTO>>("GitUsage.CreateAccount", async (req) =>
        {
            _logger.LogInformation("Received register request \n Attempt gitea registration...");
            _logger.LogInformation(await _gitUsage.TryCreateAccount(req.Data) ? "Success" : "Failed");
        });
    }

}