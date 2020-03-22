using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Contacts.Clients;
using Crm.v1.Clients.Contacts.Models;
using Crm.v1.Clients.Contacts.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Contacts
{
    public class ContactAttributesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IContactAttributesClient _contactAttributesClient;

        public ContactAttributesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IContactAttributesClient contactAttributesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _contactAttributesClient = contactAttributesClient;
        }

        [Fact]
        public async Task WhenGetTypes_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var types = await _contactAttributesClient.GetTypesAsync(accessToken);

            Assert.NotEmpty(types);
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attributeId = (await _create.ContactAttribute.BuildAsync()).Id;

            var attribute = await _contactAttributesClient.GetAsync(accessToken, attributeId);

            Assert.NotNull(attribute);
            Assert.Equal(attributeId, attribute.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attributeIds = (
                    await Task.WhenAll(
                        _create.ContactAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync(),
                        _create.ContactAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var attributes = await _contactAttributesClient.GetListAsync(accessToken, attributeIds);

            Assert.NotEmpty(attributes);
            Assert.Equal(attributeIds.Count, attributes.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var key = "Test".WithGuid();
            await Task.WhenAll(
                _create.ContactAttribute
                    .WithType(AttributeType.Text)
                    .WithKey(key)
                    .BuildAsync());
            var filterTypes = new List<AttributeType> {AttributeType.Text};

            var request = new ContactAttributeGetPagedListRequest
            {
                Key = key,
                Types = filterTypes,
            };

            var response = await _contactAttributesClient.GetPagedListAsync(accessToken, request);

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

            var attribute = new ContactAttribute
            {
                Type = AttributeType.Text,
                Key = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdAttributeId = await _contactAttributesClient.CreateAsync(accessToken, attribute);

            var createdAttribute = await _contactAttributesClient.GetAsync(accessToken, createdAttributeId);

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

            var attribute = await _create.ContactAttribute
                .WithType(AttributeType.Text)
                .WithKey("Test".WithGuid())
                .BuildAsync();

            attribute.Type = AttributeType.Link;
            attribute.Key = "Test".WithGuid();
            attribute.IsDeleted = true;

            await _contactAttributesClient.UpdateAsync(accessToken, attribute);

            var updatedAttribute = await _contactAttributesClient.GetAsync(accessToken, attribute.Id);

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
                        _create.ContactAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync(),
                        _create.ContactAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _contactAttributesClient.DeleteAsync(accessToken, attributeIds);

            var attributes = await _contactAttributesClient.GetListAsync(accessToken, attributeIds);

            Assert.All(attributes, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attributeIds = (
                    await Task.WhenAll(
                        _create.ContactAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync(),
                        _create.ContactAttribute
                            .WithKey("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _contactAttributesClient.RestoreAsync(accessToken, attributeIds);

            var attributes = await _contactAttributesClient.GetListAsync(accessToken, attributeIds);

            Assert.All(attributes, x => Assert.False(x.IsDeleted));
        }
    }
}