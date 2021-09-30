using System.Threading.Tasks;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Orders.Clients;
using Crm.v1.Clients.Orders.Models;

namespace Crm.Tests.All.Builders.Orders
{
    public class OrderStatusBuilder : IOrderStatusBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IOrderStatusesClient _orderStatusesClient;
        private readonly OrderStatus _status;

        public OrderStatusBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IOrderStatusesClient orderStatusesClient)
        {
            _orderStatusesClient = orderStatusesClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _status = new OrderStatus
            {
                Name = "Test".WithGuid(),
                IsFinish = false,
                IsDeleted = false
            };
        }

        public OrderStatusBuilder WithName(string name)
        {
            _status.Name = name;

            return this;
        }

        public OrderStatusBuilder AsFinish()
        {
            _status.IsFinish = true;

            return this;
        }

        public OrderStatusBuilder AsDeleted()
        {
            _status.IsDeleted = true;

            return this;
        }

        public async Task<OrderStatus> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var id = await _orderStatusesClient.CreateAsync(_status, headers);

            return await _orderStatusesClient.GetAsync(id, headers);
        }
    }
}
