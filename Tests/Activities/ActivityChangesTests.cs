using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.V1.Clients.Activities.Clients;
using Crm.V1.Clients.Activities.Models;
using Crm.V1.Clients.Activities.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Activities
{
    public class ActivityChangesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IActivitiesClient _activitiesClient;
        private readonly IActivityChangesClient _activityChangesClient;

        public ActivityChangesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IActivitiesClient activitiesClient,
            IActivityChangesClient activityChangesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _activitiesClient = activitiesClient;
            _activityChangesClient = activityChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.ActivityType.BuildAsync();
            var status = await _create.ActivityStatus.BuildAsync();
            var activity = await _create.Activity
                .WithTypeId(type.Id)
                .WithStatusId(status.Id)
                .BuildAsync();

            activity.Name = "Test".WithGuid();
            activity.IsDeleted = true;

            await _activitiesClient.UpdateAsync(accessToken, activity);

            var request = new ActivityChangeGetPagedListRequest
            {
                ActivityId = activity.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _activityChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.ActivityId == activity.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<Activity>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<Activity>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<Activity>().IsDeleted);
        }
    }
}
