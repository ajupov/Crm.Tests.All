using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Orders.Clients;
using Crm.v1.Clients.Orders.Models;
using Crm.v1.Clients.Orders.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Orders
{
    public class OrderChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IOrdersClient _ordersClient;
        private readonly IOrderChangesClient _orderChangesClient;

        public OrderChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IOrdersClient ordersClient,
            IOrderChangesClient orderChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _ordersClient = ordersClient;
            _orderChangesClient = orderChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.OrderType.BuildAsync();
            var status = await _create.OrderStatus.BuildAsync();
            var order = await _create.Order
                .WithTypeId(type.Id)
                .WithStatusId(status.Id)
                .BuildAsync();

            order.IsDeleted = true;

            await _ordersClient.UpdateAsync(order, headers);

            var request = new OrderChangeGetPagedListRequest
            {
                OrderId = order.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _orderChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.OrderId == order.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<Order>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<Order>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<Order>().IsDeleted);
        }
    }
}
