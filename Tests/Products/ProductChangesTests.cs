using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.V1.Clients.Products.Clients;
using Crm.V1.Clients.Products.Models;
using Crm.V1.Clients.Products.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Products
{
    public class ProductChangesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IProductsClient _productsClient;
        private readonly IProductChangesClient _productChangesClient;

        public ProductChangesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IProductsClient productsClient,
            IProductChangesClient productChangesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _productsClient = productsClient;
            _productChangesClient = productChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var status = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(status.Id)
                .BuildAsync();

            product.IsHidden = true;

            await _productsClient.UpdateAsync(accessToken, product);

            var request = new ProductChangeGetPagedListRequest
            {
                ProductId = product.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _productChangesClient.GetPagedListAsync(accessToken, request);

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
