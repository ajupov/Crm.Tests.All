using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Orders.Clients;
using Crm.v1.Clients.Orders.Models;
using Xunit;

namespace Crm.Tests.All.Tests.Orders
{
    public class OrderAttributesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IOrderAttributesClient _orderAttributesClient;

        public OrderAttributesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IOrderAttributesClient orderAttributesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _orderAttributesClient = orderAttributesClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attributeId = (await _create.OrderAttribute.BuildAsync()).Id;

            var attribute = await _orderAttributesClient.GetAsync(attributeId, headers);

            Assert.NotNull(attribute);
            Assert.Equal(attributeId, attribute.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attributeIds = (
                    await Task.WhenAll(
                        _create.OrderAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync(),
                        _create.OrderAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var attributes = await _orderAttributesClient.GetListAsync(attributeIds, headers);

            Assert.NotEmpty(attributes);
            Assert.Equal(attributeIds.Count, attributes.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var key = "Test".WithGuid();
            await Task.WhenAll(
                _create.OrderAttribute
                    .WithType(AttributeType.Text)
                    .WithKey(key)
                    .BuildAsync());
            var filterTypes = new List<AttributeType> { AttributeType.Text };

            var request = new OrderAttributeGetPagedListRequest
            {
                Key = key,
                Types = filterTypes,
            };

            var response = await _orderAttributesClient.GetPagedListAsync(request, headers);

            var results = response.Attributes
                .Skip(1)
                .Zip(response.Attributes, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Attributes);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = new OrderAttribute
            {
                Id = Guid.NewGuid(),
                Type = AttributeType.Text,
                Key = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdAttributeId = await _orderAttributesClient.CreateAsync(attribute, headers);

            var createdAttribute = await _orderAttributesClient.GetAsync(createdAttributeId, headers);

            Assert.NotNull(createdAttribute);
            Assert.Equal(createdAttributeId, createdAttribute.Id);
            Assert.Equal(attribute.Type, createdAttribute.Type);
            Assert.Equal(attribute.Key, createdAttribute.Key);
            Assert.Equal(attribute.IsDeleted, createdAttribute.IsDeleted);
            Assert.True(createdAttribute.CreateDateTime.IsMoreThanMinValue());
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = await _create.OrderAttribute
                .WithType(AttributeType.Text)
                .WithKey("Test".WithGuid())
                .BuildAsync();

            attribute.Type = AttributeType.Link;
            attribute.Key = "Test".WithGuid();
            attribute.IsDeleted = true;

            await _orderAttributesClient.UpdateAsync(attribute, headers);

            var updatedAttribute = await _orderAttributesClient.GetAsync(attribute.Id, headers);

            Assert.Equal(attribute.Type, updatedAttribute.Type);
            Assert.Equal(attribute.Key, updatedAttribute.Key);
            Assert.Equal(attribute.IsDeleted, updatedAttribute.IsDeleted);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attributeIds = (
                    await Task.WhenAll(
                        _create.OrderAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync(),
                        _create.OrderAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _orderAttributesClient.DeleteAsync(attributeIds, headers);

            var attributes = await _orderAttributesClient.GetListAsync(attributeIds, headers);

            Assert.All(attributes, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attributeIds = (
                    await Task.WhenAll(
                        _create.OrderAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync(),
                        _create.OrderAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _orderAttributesClient.RestoreAsync(attributeIds, headers);

            var attributes = await _orderAttributesClient.GetListAsync(attributeIds, headers);

            Assert.All(attributes, x => Assert.False(x.IsDeleted));
        }
    }
}
