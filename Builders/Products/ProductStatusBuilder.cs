using System.Threading.Tasks;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;

namespace Crm.Tests.All.Builders.Products
{
    public class ProductStatusBuilder : IProductStatusBuilder
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly IProductStatusesClient _productStatusesClient;
        private readonly ProductStatus _status;

        public ProductStatusBuilder(IAccessTokenGetter accessTokenGetter, IProductStatusesClient productStatusesClient)
        {
            _productStatusesClient = productStatusesClient;
            _accessTokenGetter = accessTokenGetter;
            _status = new ProductStatus
            {
                Name = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public ProductStatusBuilder WithName(string name)
        {
            _status.Name = name;

            return this;
        }

        public ProductStatusBuilder IsDeleted()
        {
            _status.IsDeleted = true;

            return this;
        }

        public async Task<ProductStatus> BuildAsync()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var id = await _productStatusesClient.CreateAsync(accessToken, _status);

            return await _productStatusesClient.GetAsync(accessToken, id);
        }
    }
}