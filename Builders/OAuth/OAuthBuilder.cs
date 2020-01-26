using System.Threading.Tasks;
using Crm.Tests.All.Settings;
using Crm.v1.Clients.OAuth.Clients;
using Crm.v1.Clients.OAuth.Models;
using Microsoft.Extensions.Options;

namespace Crm.Tests.All.Builders.OAuth
{
    public class OAuthBuilder : IOAuthBuilder
    {
        private readonly OAuthSettings _oauthSettings;
        private readonly IOAuthClient _oauthClient;

        public OAuthBuilder(IOptions<OAuthSettings> options, IOAuthClient oauthClient)
        {
            _oauthSettings = options.Value;
            _oauthClient = oauthClient;
        }

        public Task<Tokens> BuildAsync()
        {
            return _oauthClient.GetTokensAsync(_oauthSettings.Username, _oauthSettings.Password);
        }
    }
}