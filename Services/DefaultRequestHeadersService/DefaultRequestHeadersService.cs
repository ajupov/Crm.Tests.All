using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crm.Tests.All.Settings;
using Crm.v1.Clients.OAuth.Clients;
using Microsoft.Extensions.Options;

namespace Crm.Tests.All.Services.DefaultRequestHeadersService
{
    public class DefaultRequestHeadersService : IDefaultRequestHeadersService
    {
        private readonly OAuthSettings _oauthSettings;
        private readonly IOAuthClient _oauthClient;

        private readonly SemaphoreSlim _semaphore = new (1);
        private bool _isLoaded;
        private Dictionary<string, string> _data;

        public DefaultRequestHeadersService(IOptions<OAuthSettings> options, IOAuthClient oauthClient)
        {
            _oauthSettings = options.Value;
            _oauthClient = oauthClient;
        }

        public async Task<Dictionary<string, string>> GetAsync()
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

        private async Task<Dictionary<string, string>> GetInternalAsync()
        {
            var tokens = await _oauthClient.GetTokensAsync(_oauthSettings.Username, _oauthSettings.Password, null);

            return new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {tokens.AccessToken}" }
            };
        }
    }
}
