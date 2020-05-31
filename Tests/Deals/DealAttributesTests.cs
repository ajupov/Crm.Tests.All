using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.V1.Clients.Deals.Clients;
using Crm.V1.Clients.Deals.Models;
using Crm.V1.Clients.Deals.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Deals
{
    public class DealAttributesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IDealAttributesClient _dealAttributesClient;

        public DealAttributesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IDealAttributesClient dealAttributesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _dealAttributesClient = dealAttributesClient;
        }

        [Fact]
        public async Task WhenGetTypes_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var types = await _dealAttributesClient.GetTypesAsync(accessToken);

            Assert.NotEmpty(types);
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attributeId = (await _create.DealAttribute.BuildAsync()).Id;

            var attribute = await _dealAttributesClient.GetAsync(accessToken, attributeId);

            Assert.NotNull(attribute);
            Assert.Equal(attributeId, attribute.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attributeIds = (
                    await Task.WhenAll(
                        _create.DealAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync(),
                        _create.DealAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var attributes = await _dealAttributesClient.GetListAsync(accessToken, attributeIds);

            Assert.NotEmpty(attributes);
            Assert.Equal(attributeIds.Count, attributes.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var key = "Test".WithGuid();
            await Task.WhenAll(
                _create.DealAttribute
                    .WithType(AttributeType.Text)
                    .WithKey(key)
                    .BuildAsync());
            var filterTypes = new List<AttributeType> {AttributeType.Text};

            var request = new DealAttributeGetPagedListRequest
            {
                Key = key,
                Types = filterTypes,
            };

            var response = await _dealAttributesClient.GetPagedListAsync(accessToken, request);

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

            var attribute = new DealAttribute
            {
                Type = AttributeType.Text,
                Key = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdAttributeId = await _dealAttributesClient.CreateAsync(accessToken, attribute);

            var createdAttribute = await _dealAttributesClient.GetAsync(accessToken, createdAttributeId);

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

            var attribute = await _create.DealAttribute
                .WithType(AttributeType.Text)
                .WithKey("Test".WithGuid())
                .BuildAsync();

            attribute.Type = AttributeType.Link;
            attribute.Key = "Test".WithGuid();
            attribute.IsDeleted = true;

            await _dealAttributesClient.UpdateAsync(accessToken, attribute);

            var updatedAttribute = await _dealAttributesClient.GetAsync(accessToken, attribute.Id);

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
                        _create.DealAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync(),
                        _create.DealAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _dealAttributesClient.DeleteAsync(accessToken, attributeIds);

            var attributes = await _dealAttributesClient.GetListAsync(accessToken, attributeIds);

            Assert.All(attributes, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attributeIds = (
                    await Task.WhenAll(
                        _create.DealAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync(),
                        _create.DealAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _dealAttributesClient.RestoreAsync(accessToken, attributeIds);

            var attributes = await _dealAttributesClient.GetListAsync(accessToken, attributeIds);

            Assert.All(attributes, x => Assert.False(x.IsDeleted));
        }
    }
}
