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
    public class ActivityStatusChangesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IActivityStatusesClient _activityStatusesClient;
        private readonly IActivityStatusChangesClient _statusChangesClient;

        public ActivityStatusChangesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IActivityStatusesClient activityStatusesClient,
            IActivityStatusChangesClient statusChangesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _activityStatusesClient = activityStatusesClient;
            _statusChangesClient = statusChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var status = await _create.ActivityStatus.BuildAsync();

            status.Name = "Test".WithGuid();
            status.IsDeleted = true;

            await _activityStatusesClient.UpdateAsync(accessToken, status);

            var request = new ActivityStatusChangeGetPagedListRequestParameter
            {
                StatusId = status.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var changes = await _statusChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(changes);
            Assert.True(changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(changes.All(x => x.StatusId == status.Id));
            Assert.True(changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(changes.First().OldValueJson.IsEmpty());
            Assert.True(!changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(changes.First().NewValueJson.FromJsonString<ActivityStatus>());
            Assert.True(!changes.Last().OldValueJson.IsEmpty());
            Assert.True(!changes.Last().NewValueJson.IsEmpty());
            Assert.False(changes.Last().OldValueJson.FromJsonString<ActivityStatus>().IsDeleted);
            Assert.True(changes.Last().NewValueJson.FromJsonString<ActivityStatus>().IsDeleted);
            Assert.Equal(changes.Last().NewValueJson.FromJsonString<ActivityStatus>().Name, status.Name);
        }
    }
}