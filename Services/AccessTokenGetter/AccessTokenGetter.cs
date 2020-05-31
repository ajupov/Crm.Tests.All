using System.Threading;
using System.Threading.Tasks;
using Crm.Tests.All.Settings;
using Crm.V1.Clients.OAuth.Clients;
using Microsoft.Extensions.Options;

namespace Crm.Tests.All.Services.AccessTokenGetter
{
    public class AccessTokenGetter : IAccessTokenGetter
    {
        private readonly OAuthSettings _oauthSettings;
        private readonly IOAuthClient _oauthClient;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private bool _isLoaded;
        private string _data;

        public AccessTokenGetter(IOptions<OAuthSettings> options, IOAuthClient oauthClient)
        {
            _oauthSettings = options.Value;
            _oauthClient = oauthClient;
        }

        public async Task<string> GetAsync()
        {
            if (_isLoaded)
            {
                return _data;
            }

            await _semaphore.WaitAsync();

            try
            {
                if (_isLoaded)
                {
                    return _data;
                }

                _data = await GetInternalAsync();
                _isLoaded = true;
            }
            finally
            {
                _semaphore.Release();
            }

            return _data;
        }

        private async Task<string> GetInternalAsync()
        {
            var tokens = await _oauthClient.GetTokensAsync(_oauthSettings.Username, _oauthSettings.Password);

            return tokens.AccessToken;
        }
    }
}
