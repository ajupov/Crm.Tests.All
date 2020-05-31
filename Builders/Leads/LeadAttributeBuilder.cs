using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.V1.Clients.Leads.Clients;
using Crm.V1.Clients.Leads.Models;

namespace Crm.Tests.All.Builders.Leads
{
    public class LeadAttributeBuilder : ILeadAttributeBuilder
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ILeadAttributesClient _leadAttributesClient;
        private readonly LeadAttribute _attribute;

        public LeadAttributeBuilder(IAccessTokenGetter accessTokenGetter, ILeadAttributesClient leadAttributesClient)
        {
            _leadAttributesClient = leadAttributesClient;
            _accessTokenGetter = accessTokenGetter;
            _attribute = new LeadAttribute
            {
                Type = AttributeType.Text,
                Key = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public LeadAttributeBuilder WithType(AttributeType type)
        {
            _attribute.Type = type;

            return this;
        }

        public LeadAttributeBuilder WithKey(string key)
        {
            _attribute.Key = key;

            return this;
        }

        public LeadAttributeBuilder AsDeleted()
        {
            _attribute.IsDeleted = true;

            return this;
        }

        public async Task<LeadAttribute> BuildAsync()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var id = await _leadAttributesClient.CreateAsync(accessToken, _attribute);

            return await _leadAttributesClient.GetAsync(accessToken, id);
        }
    }
}
