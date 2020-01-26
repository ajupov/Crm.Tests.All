using System.Threading.Tasks;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;

namespace Crm.Tests.All.Builders.Products
{
    public class ProductCategoryBuilder : IProductCategoryBuilder
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly IProductCategoriesClient _productCategoriesClient;
        private readonly ProductCategory _category;

        public ProductCategoryBuilder(
            IAccessTokenGetter accessTokenGetter,
            IProductCategoriesClient productCategoriesClient)
        {
            _productCategoriesClient = productCategoriesClient;
            _accessTokenGetter = accessTokenGetter;
            _category = new ProductCategory
            {
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
            var accessToken = await _accessTokenGetter.GetAsync();

            var id = await _productCategoriesClient.CreateAsync(accessToken, _category);

            return await _productCategoriesClient.GetAsync(accessToken, id);
        }
    }
}