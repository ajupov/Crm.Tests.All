using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Customers.Clients;
using Crm.v1.Clients.Customers.Models;
using Xunit;

namespace Crm.Tests.All.Tests.Customers
{
    public class CustomerSourceChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ICustomerSourcesClient _customerSourcesClient;
        private readonly ICustomerSourceChangesClient _sourceChangesClient;

        public CustomerSourceChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ICustomerSourcesClient customerSourcesClient,
            ICustomerSourceChangesClient sourceChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _customerSourcesClient = customerSourcesClient;
            _sourceChangesClient = sourceChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var source = await _create.CustomerSource.BuildAsync();

            source.Name = "Test".WithGuid();
            source.IsDeleted = true;

            await _customerSourcesClient.UpdateAsync(source, headers);

            var request = new CustomerSourceChangeGetPagedListRequest
            {
                SourceId = source.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _sourceChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.SourceId == source.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<CustomerSource>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<CustomerSource>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<CustomerSource>().IsDeleted);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<CustomerSource>().Name, source.Name);
        }
    }
}
