using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Orders.Clients;
using Crm.v1.Clients.Orders.Models;

namespace Crm.Tests.All.Builders.Orders
{
    public class OrderBuilder : IOrderBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IOrdersClient _ordersClient;
        private readonly Order _order;

        public OrderBuilder(IDefaultRequestHeadersService defaultRequestHeadersService, IOrdersClient ordersClient)
        {
            _ordersClient = ordersClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _order = new Order
            {
                Name = "Test".WithGuid(),
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddDays(1),
                Sum = 1,
                SumWithoutDiscount = 1,
                IsDeleted = false
            };
        }

        public OrderBuilder WithTypeId(Guid typeId)
        {
            _order.TypeId = typeId;

            return this;
        }

        public OrderBuilder WithStatusId(Guid statusId)
        {
            _order.StatusId = statusId;

            return this;
        }

        public OrderBuilder WithCreateUserId(Guid createUserId)
        {
            _order.CreateUserId = createUserId;

            return this;
        }

        public OrderBuilder WithResponsibleUserId(Guid responsibleUserId)
        {
            _order.ResponsibleUserId = responsibleUserId;

            return this;
        }

        public OrderBuilder WithCustomer(Guid customerId)
        {
            _order.CustomerId = customerId;

            return this;
        }

        public OrderBuilder WithName(string name)
        {
            _order.Name = name;

            return this;
        }

        public OrderBuilder WithStartDateTime(DateTime startDateTime)
        {
            _order.StartDateTime = startDateTime;

            return this;
        }

        public OrderBuilder WithEndDateTime(DateTime endDateTime)
        {
            _order.EndDateTime = endDateTime;

            return this;
        }

        public OrderBuilder WithSum(decimal sum)
        {
            _order.Sum = sum;

            return this;
        }

        public OrderBuilder WithSumWithoutDiscount(decimal sumWithoutDiscount)
        {
            _order.SumWithoutDiscount = sumWithoutDiscount;

            return this;
        }

        public OrderBuilder AsDeleted()
        {
            _order.IsDeleted = true;

            return this;
        }

        public OrderBuilder WithItem(
            Guid productId,
            string productName,
            string productVendorCode,
            decimal price,
            decimal count)
        {
            _order.Items ??= new List<OrderItem>();
            _order.Items.Add(new OrderItem
            {
                ProductId = productId,
                ProductName = productName,
                ProductVendorCode = productVendorCode,
                Price = price,
                Count = count
            });

            return this;
        }

        public OrderBuilder WithAttributeLink(Guid attributeId, string value)
        {
            _order.AttributeLinks ??= new List<OrderAttributeLink>();
            _order.AttributeLinks.Add(new OrderAttributeLink
            {
                OrderAttributeId = attributeId,
                Value = value
            });

            return this;
        }

        public async Task<Order> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            if (_order.TypeId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_order.TypeId));
            }

            if (_order.StatusId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_order.StatusId));
            }

            var id = await _ordersClient.CreateAsync(_order, headers);

            return await _ordersClient.GetAsync(id, headers);
        }
    }
}
