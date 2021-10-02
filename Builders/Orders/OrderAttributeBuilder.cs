using System;
using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Orders.Clients;
using Crm.v1.Clients.Orders.Models;

namespace Crm.Tests.All.Builders.Orders
{
    public class OrderAttributeBuilder : IOrderAttributeBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IOrderAttributesClient _orderAttributesClient;
        private readonly OrderAttribute _attribute;

        public OrderAttributeBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IOrderAttributesClient orderAttributesClient)
        {
            _orderAttributesClient = orderAttributesClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _attribute = new OrderAttribute
            {
                Id = Guid.NewGuid(),
                Type = AttributeType.Text,
                Key = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public OrderAttributeBuilder WithType(AttributeType type)
        {
            _attribute.Type = type;

            return this;
        }

        public OrderAttributeBuilder WithKey(string key)
        {
            _attribute.Key = key;

            return this;
        }

        public OrderAttributeBuilder AsDeleted()
        {
            _attribute.IsDeleted = true;

            return this;
        }

        public async Task<OrderAttribute> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var id = await _orderAttributesClient.CreateAsync(_attribute, headers);

            return await _orderAttributesClient.GetAsync(id, headers);
        }
    }
}
