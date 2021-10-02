using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Orders.Clients;
using Crm.v1.Clients.Orders.Models;
using Crm.v1.Clients.Orders.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Orders
{
    public class OrdersTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IOrdersClient _ordersClient;

        public OrdersTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IOrdersClient ordersClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _ordersClient = ordersClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.OrderType.BuildAsync();
            var status = await _create.OrderStatus.BuildAsync();
            var orderId = (
                    await _create.Order
                        .WithTypeId(type.Id)
                        .WithStatusId(status.Id)
                        .BuildAsync())
                .Id;

            var order = await _ordersClient.GetAsync(orderId, headers);

            Assert.NotNull(order);
            Assert.Equal(orderId, order.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.OrderType.BuildAsync();
            var status = await _create.OrderStatus.BuildAsync();
            var orderIds = (
                    await Task.WhenAll(
                        _create.Order
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync(),
                        _create.Order
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var orders = await _ordersClient.GetListAsync(orderIds, headers);

            Assert.NotEmpty(orders);
            Assert.Equal(orderIds.Count, orders.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = await _create.OrderAttribute.BuildAsync();
            var type = await _create.OrderType.BuildAsync();
            var status = await _create.OrderStatus.BuildAsync();
            var value = "Test".WithGuid();
            await Task.WhenAll(
                _create.Order
                    .WithTypeId(type.Id)
                    .WithStatusId(status.Id)
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync(),
                _create.Order
                    .WithTypeId(type.Id)
                    .WithStatusId(status.Id)
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync());
            var filterAttributes = new Dictionary<Guid, string> { { attribute.Id, value } };
            var filterSourceIds = new List<Guid> { status.Id };

            var request = new OrderGetPagedListRequest
            {
                Attributes = filterAttributes,
                StatusIds = filterSourceIds
            };

            var response = await _ordersClient.GetPagedListAsync(request, headers);

            var results = response.Orders
                .Skip(1)
                .Zip(response.Orders, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Orders);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = await _create.OrderAttribute.BuildAsync();
            var type = await _create.OrderType.BuildAsync();
            var orderStatus = await _create.OrderStatus.BuildAsync();
            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                TypeId = type.Id,
                StatusId = orderStatus.Id,
                Name = "Test".WithGuid(),
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddDays(1),
                Sum = 1,
                SumWithoutDiscount = 1,
                IsDeleted = true,
                Items = new List<OrderItem>
                {
                    new ()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Count = 1,
                        Price = product.Price
                    }
                },
                AttributeLinks = new List<OrderAttributeLink>
                {
                    new ()
                    {
                        OrderAttributeId = attribute.Id,
                        Value = "Test".WithGuid()
                    }
                }
            };

            var createdOrderId = await _ordersClient.CreateAsync(order, headers);

            var createdOrder = await _ordersClient.GetAsync(createdOrderId, headers);

            Assert.NotNull(createdOrder);
            Assert.Equal(createdOrderId, createdOrder.Id);
            Assert.Equal(order.TypeId, createdOrder.TypeId);
            Assert.Equal(order.StatusId, createdOrder.StatusId);
            Assert.True(!createdOrder.CreateUserId.IsEmpty());
            Assert.Equal(order.ResponsibleUserId, createdOrder.ResponsibleUserId);
            Assert.Equal(order.Name, createdOrder.Name);
            Assert.Equal(order.StartDateTime.Date, createdOrder.StartDateTime.Date);
            Assert.Equal(order.EndDateTime?.Date, createdOrder.EndDateTime?.Date);
            Assert.Equal(order.Sum, createdOrder.Sum);
            Assert.Equal(order.SumWithoutDiscount, createdOrder.SumWithoutDiscount);
            Assert.Equal(order.IsDeleted, createdOrder.IsDeleted);
            Assert.True(createdOrder.CreateDateTime.IsMoreThanMinValue());
            Assert.NotEmpty(createdOrder.Items);
            Assert.NotEmpty(createdOrder.AttributeLinks);
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.OrderType.BuildAsync();
            var orderStatus = await _create.OrderStatus.BuildAsync();
            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();
            var attribute = await _create.OrderAttribute.BuildAsync();
            var order = await _create.Order
                .WithTypeId(type.Id)
                .WithStatusId(orderStatus.Id)
                .BuildAsync();

            order.TypeId = type.Id;
            order.StatusId = orderStatus.Id;
            order.Name = "Test".WithGuid();
            order.StartDateTime = DateTime.UtcNow;
            order.EndDateTime = DateTime.UtcNow.AddDays(1);
            order.Sum = 1;
            order.SumWithoutDiscount = 1;
            order.IsDeleted = true;
            order.Items.Add(
                new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Count = 1,
                    Price = product.Price
                });
            order.AttributeLinks.Add(
                new OrderAttributeLink
                {
                    OrderAttributeId = attribute.Id,
                    Value = "Test".WithGuid()
                });

            await _ordersClient.UpdateAsync(order, headers);

            var updatedOrder = await _ordersClient.GetAsync(order.Id, headers);

            Assert.Equal(order.AccountId, updatedOrder.AccountId);
            Assert.Equal(order.TypeId, updatedOrder.TypeId);
            Assert.Equal(order.StatusId, updatedOrder.StatusId);
            Assert.Equal(order.CreateUserId, updatedOrder.CreateUserId);
            Assert.Equal(order.ResponsibleUserId, updatedOrder.ResponsibleUserId);
            Assert.Equal(order.Name, updatedOrder.Name);
            Assert.Equal(order.StartDateTime.Date, updatedOrder.StartDateTime.Date);
            Assert.Equal(order.EndDateTime?.Date, updatedOrder.EndDateTime?.Date);
            Assert.Equal(order.Sum, updatedOrder.Sum);
            Assert.Equal(order.SumWithoutDiscount, updatedOrder.SumWithoutDiscount);
            Assert.Equal(order.IsDeleted, updatedOrder.IsDeleted);
            Assert.Equal(order.Items.Single().ProductId, updatedOrder.Items.Single().ProductId);
            Assert.Equal(order.Items.Single().Count, updatedOrder.Items.Single().Count);
            Assert.Equal(order.Items.Single().Price, updatedOrder.Items.Single().Price);
            Assert.Equal(order.AttributeLinks.Single().Value, updatedOrder.AttributeLinks.Single().Value);
            Assert.Equal(
                order.AttributeLinks.Single().OrderAttributeId,
                updatedOrder.AttributeLinks.Single().OrderAttributeId);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.OrderType.BuildAsync();
            var status = await _create.OrderStatus.BuildAsync();
            var orderIds = (
                    await Task.WhenAll(
                        _create.Order
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync(),
                        _create.Order
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _ordersClient.DeleteAsync(orderIds, headers);

            var orders = await _ordersClient.GetListAsync(orderIds, headers);

            Assert.All(orders, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.OrderType.BuildAsync();
            var status = await _create.OrderStatus.BuildAsync();
            var orderIds = (
                    await Task.WhenAll(
                        _create.Order
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync(),
                        _create.Order
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _ordersClient.RestoreAsync(orderIds, headers);

            var orders = await _ordersClient.GetListAsync(orderIds, headers);

            Assert.All(orders, x => Assert.False(x.IsDeleted));
        }
    }
}
