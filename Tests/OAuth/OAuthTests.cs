using System.Threading.Tasks;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Settings;
using Crm.v1.Clients.OAuth.Clients;
using Microsoft.Extensions.Options;
using Xunit;

namespace Crm.Tests.All.Tests.OAuth
{
    public class OAuthTests
    {
        private readonly OAuthSettings _oauthSettings;
        private readonly ICreate _create;
        private readonly IOAuthClient _oauthClient;

        public OAuthTests(IOptions<OAuthSettings> options, ICreate create, IOAuthClient oauthClient)
        {
            _oauthSettings = options.Value;
            _create = create;
            _oauthClient = oauthClient;
        }

        [Fact]
        public async Task WhenGetTokens_ThenSuccess()
        {
            var tokens = await _oauthClient.GetTokensAsync(_oauthSettings.Username, _oauthSettings.Password, null);

            Assert.NotNull(tokens.AccessToken);
            Assert.NotNull(tokens.RefreshToken);
            Assert.True(tokens.ExpiresIn > 0);
            Assert.NotNull(tokens.TokenType);
        }

        [Fact]
        public async Task WhenRefreshTokens_ThenSuccess()
        {
            var tokens = await _create.OAuth.BuildAsync();

            var newTokens = await _oauthClient.RefreshTokensAsync(tokens.RefreshToken, null);

            Assert.NotNull(newTokens.AccessToken);
            Assert.NotNull(newTokens.RefreshToken);
            Assert.True(newTokens.ExpiresIn > 0);
            Assert.NotNull(newTokens.TokenType);
        }
    }
}
