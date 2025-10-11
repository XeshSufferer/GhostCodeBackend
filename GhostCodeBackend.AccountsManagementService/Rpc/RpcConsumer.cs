using GhostCodeBackend.Shared.RPC.MessageBroker;

namespace GhostCodeBakend.AccountsManagementService.Rpc;

public class RpcConsumer
{

    private readonly IRabbitMQService _rabbit;

    public RpcConsumer(IRabbitMQService rabbit)
    {
        _rabbit = rabbit;
    }
    
    public async Task InitConsuming()
    {
        //await _rabbit.StartConsumingAsync()
    }
}