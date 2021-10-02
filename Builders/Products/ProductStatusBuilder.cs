using System;
using System.Threading.Tasks;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;

namespace Crm.Tests.All.Builders.Products
{
    public class ProductStatusBuilder : IProductStatusBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IProductStatusesClient _productStatusesClient;
        private readonly ProductStatus _status;

        public ProductStatusBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IProductStatusesClient productStatusesClient)
        {
            _productStatusesClient = productStatusesClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _status = new ProductStatus
            {
                Id = Guid.NewGuid(),
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
            var headers = await _defaultRequestHeadersService.GetAsync();

            var id = await _productStatusesClient.CreateAsync(_status, headers);

            return await _productStatusesClient.GetAsync(id, headers);
        }
    }
}
