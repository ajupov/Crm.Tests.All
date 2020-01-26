using System.Threading.Tasks;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.v1.Clients.Deals.Clients;
using Crm.v1.Clients.Deals.Models;

namespace Crm.Tests.All.Builders.Deals
{
    public class DealTypeBuilder : IDealTypeBuilder
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly IDealTypesClient _dealTypesClient;
        private readonly DealType _type;

        public DealTypeBuilder(IAccessTokenGetter accessTokenGetter, IDealTypesClient dealTypesClient)
        {
            _dealTypesClient = dealTypesClient;
            _accessTokenGetter = accessTokenGetter;
            _type = new DealType
            {
                Name = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public DealTypeBuilder WithName(string name)
        {
            _type.Name = name;

            return this;
        }

        public DealTypeBuilder AsDeleted()
        {
            _type.IsDeleted = true;

            return this;
        }

        public async Task<DealType> BuildAsync()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var id = await _dealTypesClient.CreateAsync(accessToken, _type);

            return await _dealTypesClient.GetAsync(accessToken, id);
        }
    }
}