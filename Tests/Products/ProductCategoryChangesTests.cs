using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;
using Crm.v1.Clients.Products.RequestParameters;
using Xunit;

namespace Crm.Tests.All.Tests.Products
{
    public class ProductCategoryChangesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IProductCategoriesClient _productCategoriesClient;
        private readonly IProductCategoryChangesClient _groupChangesClient;

        public ProductCategoryChangesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IProductCategoriesClient productCategoriesClient,
            IProductCategoryChangesClient groupChangesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _productCategoriesClient = productCategoriesClient;
            _groupChangesClient = groupChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var category = await _create.ProductCategory.BuildAsync();

            category.Name = "Test".WithGuid();
            category.IsDeleted = true;

            await _productCategoriesClient.UpdateAsync(accessToken, category);

            var request = new ProductCategoryChangeGetPagedListRequestParameter
            {
                CategoryId = category.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var changes = await _groupChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(changes);
            Assert.True(changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(changes.All(x => x.CategoryId == category.Id));
            Assert.True(changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(changes.First().OldValueJson.IsEmpty());
            Assert.True(!changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(changes.First().NewValueJson.FromJsonString<ProductCategory>());
            Assert.True(!changes.Last().OldValueJson.IsEmpty());
            Assert.True(!changes.Last().NewValueJson.IsEmpty());
            Assert.False(changes.Last().OldValueJson.FromJsonString<ProductCategory>().IsDeleted);
            Assert.True(changes.Last().NewValueJson.FromJsonString<ProductCategory>().IsDeleted);
            Assert.Equal(changes.Last().NewValueJson.FromJsonString<ProductCategory>().Name, category.Name);
        }
    }
}