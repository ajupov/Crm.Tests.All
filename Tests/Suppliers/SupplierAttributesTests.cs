using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Suppliers.Clients;
using Crm.v1.Clients.Suppliers.Models;
using Xunit;

namespace Crm.Tests.All.Tests.Suppliers
{
    public class SupplierAttributesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ISupplierAttributesClient _supplierAttributesClient;

        public SupplierAttributesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ISupplierAttributesClient supplierAttributesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _supplierAttributesClient = supplierAttributesClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attributeId = (await _create.SupplierAttribute.BuildAsync()).Id;

            var attribute = await _supplierAttributesClient.GetAsync(attributeId, headers);

            Assert.NotNull(attribute);
            Assert.Equal(attributeId, attribute.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attributeIds = (
                    await Task.WhenAll(
                        _create.SupplierAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync(),
                        _create.SupplierAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var attributes = await _supplierAttributesClient.GetListAsync(attributeIds, headers);

            Assert.NotEmpty(attributes);
            Assert.Equal(attributeIds.Count, attributes.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var key = "Test".WithGuid();
            await Task.WhenAll(
                _create.SupplierAttribute
                    .WithType(AttributeType.Text)
                    .WithKey(key)
                    .BuildAsync());
            var filterTypes = new List<AttributeType> { AttributeType.Text };

            var request = new SupplierAttributeGetPagedListRequest
            {
                Key = key,
                Types = filterTypes
            };

            var response = await _supplierAttributesClient.GetPagedListAsync(request, headers);

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

            var attribute = new SupplierAttribute
            {
                Id = Guid.NewGuid(),
                Type = AttributeType.Text,
                Key = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdAttributeId = await _supplierAttributesClient.CreateAsync(attribute, headers);

            var createdAttribute = await _supplierAttributesClient.GetAsync(createdAttributeId, headers);

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

            var attribute = await _create.SupplierAttribute
                .WithType(AttributeType.Text)
                .WithKey("Test".WithGuid())
                .BuildAsync();

            attribute.Type = AttributeType.Link;
            attribute.Key = "Test".WithGuid();
            attribute.IsDeleted = true;

            await _supplierAttributesClient.UpdateAsync(attribute, headers);

            var updatedAttribute = await _supplierAttributesClient.GetAsync(attribute.Id, headers);

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
                        _create.SupplierAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync(),
                        _create.SupplierAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _supplierAttributesClient.DeleteAsync(attributeIds, headers);

            var attributes = await _supplierAttributesClient.GetListAsync(attributeIds, headers);

            Assert.All(attributes, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attributeIds = (
                    await Task.WhenAll(
                        _create.SupplierAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync(),
                        _create.SupplierAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _supplierAttributesClient.RestoreAsync(attributeIds, headers);

            var attributes = await _supplierAttributesClient.GetListAsync(attributeIds, headers);

            Assert.All(attributes, x => Assert.False(x.IsDeleted));
        }
    }
}
