using Zen.IdentityServer.Models;
using Zen.IdentityServer.Services;

namespace IdentityServer.UnitTests.Common;

class MockTokenCreationService : ITokenCreationService
{
    public string Token { get; set; }

    public Task<string> CreateTokenAsync(Token token)
    {
        return Task.FromResult(Token);
    }
}
