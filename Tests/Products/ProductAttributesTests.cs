using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;
using Crm.v1.Clients.Products.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Products
{
    public class ProductAttributesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IProductAttributesClient _productAttributesClient;

        public ProductAttributesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IProductAttributesClient productAttributesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _productAttributesClient = productAttributesClient;
        }

        [Fact]
        public async Task WhenGetTypes_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var types = await _productAttributesClient.GetTypesAsync(accessToken);

            Assert.NotEmpty(types);
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attributeId = (await _create.ProductAttribute.BuildAsync()).Id;

            var attribute = await _productAttributesClient.GetAsync(accessToken, attributeId);

            Assert.NotNull(attribute);
            Assert.Equal(attributeId, attribute.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attributeIds = (
                    await Task.WhenAll(
                        _create.ProductAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync(),
                        _create.ProductAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var attributes = await _productAttributesClient.GetListAsync(accessToken, attributeIds);

            Assert.NotEmpty(attributes);
            Assert.Equal(attributeIds.Count, attributes.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var key = "Test".WithGuid();
            await Task.WhenAll(
                _create.ProductAttribute
                    .WithType(AttributeType.Text)
                    .WithKey(key)
                    .BuildAsync());
            var filterTypes = new List<AttributeType> {AttributeType.Text};

            var request = new ProductAttributeGetPagedListRequest
            {
                Key = key,
                Types = filterTypes,
            };

            var response = await _productAttributesClient.GetPagedListAsync(accessToken, request);

            var results = response.Attributes
                .Skip(1)
                .Zip(response.Attributes, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Attributes);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attribute = new ProductAttribute
            {
                Type = AttributeType.Text,
                Key = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdAttributeId = await _productAttributesClient.CreateAsync(accessToken, attribute);

            var createdAttribute = await _productAttributesClient.GetAsync(accessToken, createdAttributeId);

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
            var accessToken = await _accessTokenGetter.GetAsync();

            var attribute = await _create.ProductAttribute
                .WithType(AttributeType.Text)
                .WithKey("Test".WithGuid())
                .BuildAsync();

            attribute.Type = AttributeType.Link;
            attribute.Key = "Test".WithGuid();
            attribute.IsDeleted = true;

            await _productAttributesClient.UpdateAsync(accessToken, attribute);

            var updatedAttribute = await _productAttributesClient.GetAsync(accessToken, attribute.Id);

            Assert.Equal(attribute.Type, updatedAttribute.Type);
            Assert.Equal(attribute.Key, updatedAttribute.Key);
            Assert.Equal(attribute.IsDeleted, updatedAttribute.IsDeleted);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attributeIds = (
                    await Task.WhenAll(
                        _create.ProductAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync(),
                        _create.ProductAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _productAttributesClient.DeleteAsync(accessToken, attributeIds);

            var attributes = await _productAttributesClient.GetListAsync(accessToken, attributeIds);

            Assert.All(attributes, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attributeIds = (
                    await Task.WhenAll(
                        _create.ProductAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync(),
                        _create.ProductAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _productAttributesClient.RestoreAsync(accessToken, attributeIds);

            var attributes = await _productAttributesClient.GetListAsync(accessToken, attributeIds);

            Assert.All(attributes, x => Assert.False(x.IsDeleted));
        }
    }
}