using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Customers.Clients;
using Crm.v1.Clients.Customers.Models;
using Xunit;

namespace Crm.Tests.All.Tests.Customers
{
    public class CustomerChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ICustomersClient _customersClient;
        private readonly ICustomerChangesClient _customerChangesClient;

        public CustomerChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ICustomersClient customersClient,
            ICustomerChangesClient customerChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _customersClient = customersClient;
            _customerChangesClient = customerChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var source = await _create.CustomerSource.BuildAsync();
            var customer = await _create.Customer
                .WithSourceId(source.Id)
                .BuildAsync();

            customer.IsDeleted = true;

            await _customersClient.UpdateAsync(customer, headers);

            var request = new CustomerChangeGetPagedListRequest
            {
                CustomerId = customer.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _customerChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.CustomerId == customer.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<Customer>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<Customer>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<Customer>().IsDeleted);
        }
    }
}
