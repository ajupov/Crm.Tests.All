using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Activities.Clients;
using Crm.v1.Clients.Activities.Models;
using Crm.v1.Clients.Activities.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Activities
{
    public class ActivityAttributeChangesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IActivityAttributesClient _activityAttributesClient;
        private readonly IActivityAttributeChangesClient _attributeChangesClient;

        public ActivityAttributeChangesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IActivityAttributesClient activityAttributesClient,
            IActivityAttributeChangesClient attributeChangesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _activityAttributesClient = activityAttributesClient;
            _attributeChangesClient = attributeChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attribute = await _create.ActivityAttribute.BuildAsync();

            attribute.Type = AttributeType.Link;
            attribute.Key = "Test".WithGuid();
            attribute.IsDeleted = true;

            await _activityAttributesClient.UpdateAsync(accessToken, attribute);

            var request = new ActivityAttributeChangeGetPagedListRequest
            {
                AttributeId = attribute.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _attributeChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.AttributeId == attribute.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<ActivityAttribute>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<ActivityAttribute>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<ActivityAttribute>().IsDeleted);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<ActivityAttribute>().Key, attribute.Key);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<ActivityAttribute>().Type, attribute.Type);
        }
    }
}