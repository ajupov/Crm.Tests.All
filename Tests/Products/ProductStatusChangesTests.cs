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
using Xunit;

namespace Crm.Tests.All.Tests.Products
{
    public class ProductStatusChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IProductStatusesClient _productStatusesClient;
        private readonly IProductStatusChangesClient _productStatusChangesClient;

        public ProductStatusChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IProductStatusesClient productStatusesClient,
            IProductStatusChangesClient productStatusChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _productStatusesClient = productStatusesClient;
            _productStatusChangesClient = productStatusChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = await _create.ProductStatus.BuildAsync();

            status.Name = "Test".WithGuid();
            status.IsDeleted = true;

            await _productStatusesClient.UpdateAsync(status, headers);

            var request = new ProductStatusChangeGetPagedListRequest
            {
                StatusId = status.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _productStatusChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.StatusId == status.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<ProductStatus>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<ProductStatus>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<ProductStatus>().IsDeleted);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<ProductStatus>().Name, status.Name);
        }
    }
}
