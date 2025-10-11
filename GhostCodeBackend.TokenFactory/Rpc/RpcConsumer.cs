using GhostCodeBackend.Shared.RPC.MessageBroker;

namespace TokenFactory.Rpc;

public class RpcConsumer
{
    
    private readonly IRabbitMQService _rabbit;
    
    public RpcConsumer()
    {
        
    }
}