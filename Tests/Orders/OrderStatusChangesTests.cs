using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Orders.Clients;
using Crm.v1.Clients.Orders.Models;
using Crm.v1.Clients.Orders.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Orders
{
    public class OrderStatusChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IOrderStatusesClient _orderStatusesClient;
        private readonly IOrderStatusChangesClient _statusChangesClient;

        public OrderStatusChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IOrderStatusesClient orderStatusesClient,
            IOrderStatusChangesClient statusChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _orderStatusesClient = orderStatusesClient;
            _statusChangesClient = statusChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = await _create.OrderStatus.BuildAsync();

            status.Name = "Test".WithGuid();
            status.IsDeleted = true;

            await _orderStatusesClient.UpdateAsync(status, headers);

            var request = new OrderStatusChangeGetPagedListRequest
            {
                StatusId = status.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _statusChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.StatusId == status.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<OrderStatus>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<OrderStatus>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<OrderStatus>().IsDeleted);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<OrderStatus>().Name, status.Name);
        }
    }
}
