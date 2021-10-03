using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;
using Xunit;

namespace Crm.Tests.All.Tests.Products
{
    public class ProductAttributeChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IProductAttributesClient _productAttributesClient;
        private readonly IProductAttributeChangesClient _attributeChangesClient;

        public ProductAttributeChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IProductAttributesClient productAttributesClient,
            IProductAttributeChangesClient attributeChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _productAttributesClient = productAttributesClient;
            _attributeChangesClient = attributeChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = await _create.ProductAttribute.BuildAsync();

            attribute.Type = AttributeType.Link;
            attribute.Key = "Test".WithGuid();
            attribute.IsDeleted = true;

            await _productAttributesClient.UpdateAsync(attribute, headers);

            var request = new ProductAttributeChangeGetPagedListRequest
            {
                AttributeId = attribute.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _attributeChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.AttributeId == attribute.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<ProductAttribute>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<ProductAttribute>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<ProductAttribute>().IsDeleted);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<ProductAttribute>().Key, attribute.Key);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<ProductAttribute>().Type, attribute.Type);
        }
    }
}
