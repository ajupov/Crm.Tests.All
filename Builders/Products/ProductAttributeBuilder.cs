using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.V1.Clients.Products.Clients;
using Crm.V1.Clients.Products.Models;

namespace Crm.Tests.All.Builders.Products
{
    public class ProductAttributeBuilder : IProductAttributeBuilder
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly IProductAttributesClient _productAttributesClient;
        private readonly ProductAttribute _attribute;

        public ProductAttributeBuilder(
            IAccessTokenGetter accessTokenGetter,
            IProductAttributesClient productAttributesClient)
        {
            _productAttributesClient = productAttributesClient;
            _accessTokenGetter = accessTokenGetter;
            _attribute = new ProductAttribute
            {
                Type = AttributeType.Text,
                Key = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public ProductAttributeBuilder WithType(AttributeType type)
        {
            _attribute.Type = type;

            return this;
        }

        public ProductAttributeBuilder WithKey(string key)
        {
            _attribute.Key = key;

            return this;
        }

        public ProductAttributeBuilder AsDeleted()
        {
            _attribute.IsDeleted = true;

            return this;
        }

        public async Task<ProductAttribute> BuildAsync()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var id = await _productAttributesClient.CreateAsync(accessToken, _attribute);

            return await _productAttributesClient.GetAsync(accessToken, id);
        }
    }
}
