using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;
using Crm.v1.Clients.Products.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Products
{
    public class ProductChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IProductsClient _productsClient;
        private readonly IProductChangesClient _productChangesClient;

        public ProductChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IProductsClient productsClient,
            IProductChangesClient productChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _productsClient = productsClient;
            _productChangesClient = productChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(status.Id)
                .BuildAsync();

            product.IsHidden = true;

            await _productsClient.UpdateAsync(product, headers);

            var request = new ProductChangeGetPagedListRequest
            {
                ProductId = product.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _productChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.ProductId == product.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<Product>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<Product>().IsHidden);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<Product>().IsHidden);
        }
    }
}
