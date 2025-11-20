using GhostCodeBackend.Shared.DTO.Interservice;
using GhostCodeBackend.Shared.RPC.MessageBroker;
using GhostCodeBackend.Shared.RPC.Tracker;

namespace GhostCodeBackend.AccountsManagementService.Rpc;

public class RpcConsumer : IRpcConsumer
{

    private readonly IRabbitMQService _rabbit;
    private readonly IUniversalRequestTracker _tracker;

    public RpcConsumer(IRabbitMQService rabbit, IUniversalRequestTracker tracker)
    {
        _rabbit = rabbit;
        _tracker = tracker;
    }
    
    public async Task InitConsume()
    {
        await _rabbit.EnsureQueueExistsAsync("TokenFactory.CreateRefresh.Output");
        await _rabbit.StartConsumingAsync<Message<string>>("TokenFactory.CreateRefresh.Output", 
            (Message<string> msg) =>
        {
            _tracker.TrySetResult(msg.CorrelationId, msg);
            return Task.CompletedTask;
        });
    }
}