using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;
using Crm.v1.Clients.Products.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Products
{
    public class ProductCategoryChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IProductCategoriesClient _productCategoriesClient;
        private readonly IProductCategoryChangesClient _groupChangesClient;

        public ProductCategoryChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IProductCategoriesClient productCategoriesClient,
            IProductCategoryChangesClient groupChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _productCategoriesClient = productCategoriesClient;
            _groupChangesClient = groupChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var category = await _create.ProductCategory.BuildAsync();

            category.Name = "Test".WithGuid();
            category.IsDeleted = true;

            await _productCategoriesClient.UpdateAsync(category, headers);

            var request = new ProductCategoryChangeGetPagedListRequest
            {
                CategoryId = category.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _groupChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.CategoryId == category.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<ProductCategory>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<ProductCategory>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<ProductCategory>().IsDeleted);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<ProductCategory>().Name, category.Name);
        }
    }
}
