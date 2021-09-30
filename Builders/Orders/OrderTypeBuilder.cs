using System.Threading.Tasks;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Orders.Clients;
using Crm.v1.Clients.Orders.Models;

namespace Crm.Tests.All.Builders.Orders
{
    public class OrderTypeBuilder : IOrderTypeBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IOrderTypesClient _orderTypesClient;
        private readonly OrderType _type;

        public OrderTypeBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IOrderTypesClient orderTypesClient)
        {
            _orderTypesClient = orderTypesClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _type = new OrderType
            {
                Name = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public OrderTypeBuilder WithName(string name)
        {
            _type.Name = name;

            return this;
        }

        public OrderTypeBuilder AsDeleted()
        {
            _type.IsDeleted = true;

            return this;
        }

        public async Task<OrderType> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var id = await _orderTypesClient.CreateAsync(_type, headers);

            return await _orderTypesClient.GetAsync(id, headers);
        }
    }
}
