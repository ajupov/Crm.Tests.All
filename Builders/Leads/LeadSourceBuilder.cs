using System.Threading.Tasks;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.V1.Clients.Leads.Clients;
using Crm.V1.Clients.Leads.Models;

namespace Crm.Tests.All.Builders.Leads
{
    public class LeadSourceBuilder : ILeadSourceBuilder
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ILeadSourcesClient _leadSourcesClient;
        private readonly LeadSource _source;

        public LeadSourceBuilder(IAccessTokenGetter accessTokenGetter, ILeadSourcesClient leadSourcesClient)
        {
            _leadSourcesClient = leadSourcesClient;
            _accessTokenGetter = accessTokenGetter;
            _source = new LeadSource
            {
                Name = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public LeadSourceBuilder WithName(string name)
        {
            _source.Name = name;

            return this;
        }

        public LeadSourceBuilder AsDeleted()
        {
            _source.IsDeleted = true;

            return this;
        }

        public async Task<LeadSource> BuildAsync()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var id = await _leadSourcesClient.CreateAsync(accessToken, _source);

            return await _leadSourcesClient.GetAsync(accessToken, id);
        }
    }
}
