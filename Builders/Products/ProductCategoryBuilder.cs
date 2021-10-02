using System;
using System.Threading.Tasks;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;

namespace Crm.Tests.All.Builders.Products
{
    public class ProductCategoryBuilder : IProductCategoryBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IProductCategoriesClient _productCategoriesClient;
        private readonly ProductCategory _category;

        public ProductCategoryBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IProductCategoriesClient productCategoriesClient)
        {
            _productCategoriesClient = productCategoriesClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _category = new ProductCategory
            {
                Id = Guid.NewGuid(),
                Name = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public ProductCategoryBuilder WithName(string name)
        {
            _category.Name = name;

            return this;
        }

        public ProductCategoryBuilder IsDeleted()
        {
            _category.IsDeleted = true;

            return this;
        }

        public async Task<ProductCategory> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var id = await _productCategoriesClient.CreateAsync(_category, headers);

            return await _productCategoriesClient.GetAsync(id, headers);
        }
    }
}
