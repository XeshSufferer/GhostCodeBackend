using GhostCodeBackend.Shared.DTO.Interservice;
using GhostCodeBackend.Shared.RPC.MessageBroker;
using TokenFactory.Services;

namespace TokenFactory.Rpc;

public class RpcResponser : IRpcResponser
{
    
    private readonly IRabbitMQService _rabbit;
    private readonly IRefreshTokensService _refreshTokens;

    public RpcResponser(IRabbitMQService rabbit, IRefreshTokensService refreshTokens)
    {
        _rabbit = rabbit;
        _refreshTokens = refreshTokens;
    }

    public async Task InitResponses()
    {
        
        await _rabbit.StartConsumingAsync<Message<string>>("TokenFactory.CreateRefresh.Input",
            async (message) =>
            {
                var token = await _refreshTokens.CreateToken(message.Data);
                message.IsSuccess = token.result;

                message.Data = token.token.Token;
                
                await _rabbit.SendMessageAsync<Message<string>>(message, "TokenFactory.CreateRefresh.Output");
            });
    }
}