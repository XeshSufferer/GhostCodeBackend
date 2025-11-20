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
        
        await _rabbit.StartConsumingAsync<Message<DataForJWTWrite>>("TokenFactory.CreateRefresh.Input",
            async (message) =>
            {
                var token = await _refreshTokens.CreateToken(message.Data);
                message.IsSuccess = token.IsSuccess;

                Message<string> response = new Message<string>
                {
                    CorrelationId = message.CorrelationId,
                    IsSuccess = token.IsSuccess,
                    Data = token.Value.Token
                };
                
                await _rabbit.SendMessageAsync<Message<string>>(response, "TokenFactory.CreateRefresh.Output");
            });
    }
}