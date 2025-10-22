using GhostCodeBackend.GitUsageService.Helpers;
using GhostCodeBackend.Shared.DTO.Requests;
using Gitea.Net.Api;
using Gitea.Net.Model;

namespace GhostCodeBackend.GitUsageService.Adapters;

public class GiteaAdapter : IGiteaAdapter
{

    private readonly IAdminApi _adminApi;
    private readonly IRandomWordHelper _randomWordHelper;
    
    public GiteaAdapter(IAdminApi adminApi, IRandomWordHelper randomWordHelper)
    {
        _adminApi = adminApi;
        _randomWordHelper = randomWordHelper;
    }

    public async Task<bool> TryCreateRepository(GitRepositoryCreateRequestDTO req)
    {
        var opt = new CreateRepoOption()
        {
            Name = req.RepositoryName,
            Private = !req.IsPublic
        };
        var result = await _adminApi.AdminCreateRepoAsync(req.Username, opt);
        return result != null;
    }

    public async Task<bool> CreateAccount(RegisterRequestDTO req)
    {
        try
        {
            var email = $"{_randomWordHelper.GetRandomWord(12)}@mail.ru";
            var password = Guid.NewGuid().ToString();
            
            var opt = new CreateUserOption(
                createdAt: DateTime.UtcNow,
                email: email,
                fullName: req.Login,
                loginName: req.Login,
                mustChangePassword: true,
                password: password,
                restricted: false,
                sendNotify: false,
                sourceId: 0,
                username: req.Login,
                visibility: "public"
            );
            
            var result = await _adminApi.AdminCreateUserAsync(opt);
            return result != null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}