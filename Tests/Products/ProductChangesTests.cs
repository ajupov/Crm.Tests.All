using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;
using Crm.v1.Clients.Products.RequestParameters;
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

            var request = new ProductChangeGetPagedListRequestParameter
            {
                ProductId = product.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var changes = await _productChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(changes);
            Assert.True(changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(changes.All(x => x.ProductId == product.Id));
            Assert.True(changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(changes.First().OldValueJson.IsEmpty());
            Assert.True(!changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(changes.First().NewValueJson.FromJsonString<Product>());
            Assert.True(!changes.Last().OldValueJson.IsEmpty());
            Assert.True(!changes.Last().NewValueJson.IsEmpty());
            Assert.False(changes.Last().OldValueJson.FromJsonString<Product>().IsHidden);
            Assert.True(changes.Last().NewValueJson.FromJsonString<Product>().IsHidden);
        }
    }
}