using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;

namespace Crm.Tests.All.Builders.Products
{
    public class ProductAttributeBuilder : IProductAttributeBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IProductAttributesClient _productAttributesClient;
        private readonly ProductAttribute _attribute;

        public ProductAttributeBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IProductAttributesClient productAttributesClient)
        {
            _productAttributesClient = productAttributesClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
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
            var headers = await _defaultRequestHeadersService.GetAsync();

            var id = await _productAttributesClient.CreateAsync(_attribute, headers);

            return await _productAttributesClient.GetAsync(id, headers);
        }
    }
}
