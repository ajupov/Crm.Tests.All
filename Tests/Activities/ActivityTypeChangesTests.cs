using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Activities.Clients;
using Crm.v1.Clients.Activities.Models;
using Crm.v1.Clients.Activities.RequestParameters;
using Xunit;

namespace Crm.Tests.All.Tests.Activities
{
    public class ActivityTypeChangesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IActivityTypesClient _activityTypesClient;
        private readonly IActivityTypeChangesClient _typeChangesClient;

        public ActivityTypeChangesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IActivityTypesClient activityTypesClient,
            IActivityTypeChangesClient typeChangesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _activityTypesClient = activityTypesClient;
            _typeChangesClient = typeChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.ActivityType.BuildAsync();

            type.Name = "Test".WithGuid();
            type.IsDeleted = true;

            await _activityTypesClient.UpdateAsync(accessToken, type);

            var request = new ActivityTypeChangeGetPagedListRequestParameter
            {
                TypeId = type.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var changes = await _typeChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(changes);
            Assert.True(changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(changes.All(x => x.TypeId == type.Id));
            Assert.True(changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(changes.First().OldValueJson.IsEmpty());
            Assert.True(!changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(changes.First().NewValueJson.FromJsonString<ActivityType>());
            Assert.True(!changes.Last().OldValueJson.IsEmpty());
            Assert.True(!changes.Last().NewValueJson.IsEmpty());
            Assert.False(changes.Last().OldValueJson.FromJsonString<ActivityType>().IsDeleted);
            Assert.True(changes.Last().NewValueJson.FromJsonString<ActivityType>().IsDeleted);
            Assert.Equal(changes.Last().NewValueJson.FromJsonString<ActivityType>().Name, type.Name);
        }
    }
}