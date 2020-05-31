using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.V1.Clients.Contacts.Clients;
using Crm.V1.Clients.Contacts.Models;

namespace Crm.Tests.All.Builders.Contacts
{
    public class ContactAttributeBuilder : IContactAttributeBuilder
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly IContactAttributesClient _contactAttributesClient;
        private readonly ContactAttribute _attribute;

        public ContactAttributeBuilder(
            IAccessTokenGetter accessTokenGetter,
            IContactAttributesClient contactAttributesClient)
        {
            _contactAttributesClient = contactAttributesClient;
            _accessTokenGetter = accessTokenGetter;
            _attribute = new ContactAttribute
            {
                Type = AttributeType.Text,
                Key = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public ContactAttributeBuilder WithType(AttributeType type)
        {
            _attribute.Type = type;

            return this;
        }

        public ContactAttributeBuilder WithKey(string key)
        {
            _attribute.Key = key;

            return this;
        }

        public ContactAttributeBuilder AsDeleted()
        {
            _attribute.IsDeleted = true;

            return this;
        }

        public async Task<ContactAttribute> BuildAsync()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var id = await _contactAttributesClient.CreateAsync(accessToken, _attribute);

            return await _contactAttributesClient.GetAsync(accessToken, id);
        }
    }
}
