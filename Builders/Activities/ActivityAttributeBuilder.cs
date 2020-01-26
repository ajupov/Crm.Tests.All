using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.v1.Clients.Activities.Clients;
using Crm.v1.Clients.Activities.Models;

namespace Crm.Tests.All.Builders.Activities
{
    public class ActivityAttributeBuilder : IActivityAttributeBuilder
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly IActivityAttributesClient _activityAttributesClient;
        private readonly ActivityAttribute _attribute;

        public ActivityAttributeBuilder(
            IAccessTokenGetter accessTokenGetter,
            IActivityAttributesClient activityAttributesClient)
        {
            _activityAttributesClient = activityAttributesClient;
            _accessTokenGetter = accessTokenGetter;
            _attribute = new ActivityAttribute
            {
                Type = AttributeType.Text,
                Key = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public ActivityAttributeBuilder WithType(AttributeType type)
        {
            _attribute.Type = type;

            return this;
        }

        public ActivityAttributeBuilder WithKey(string key)
        {
            _attribute.Key = key;

            return this;
        }

        public ActivityAttributeBuilder AsDeleted()
        {
            _attribute.IsDeleted = true;

            return this;
        }

        public async Task<ActivityAttribute> BuildAsync()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var id = await _activityAttributesClient.CreateAsync(accessToken, _attribute);

            return await _activityAttributesClient.GetAsync(accessToken, id);
        }
    }
}