using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.V1.Clients.Activities.Clients;
using Crm.V1.Clients.Activities.Models;
using Crm.V1.Clients.Activities.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Activities
{
    public class ActivityCommentsTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IActivityCommentsClient _activityCommentsClient;

        public ActivityCommentsTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IActivityCommentsClient activityCommentsClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _activityCommentsClient = activityCommentsClient;
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

            await Task.WhenAll(
                _create.ActivityComment
                    .WithActivityId(activity.Id)
                    .BuildAsync(),
                _create.ActivityComment
                    .WithActivityId(activity.Id)
                    .BuildAsync());

            var request = new ActivityCommentGetPagedListRequest
            {
                ActivityId = activity.Id
            };

            var response = await _activityCommentsClient.GetPagedListAsync(accessToken, request);

            var results = response.Comments
                .Skip(1)
                .Zip(response.Comments, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Comments);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.ActivityType.BuildAsync();
            var status = await _create.ActivityStatus.BuildAsync();
            var activity = await _create.Activity
                .WithTypeId(type.Id)
                .WithStatusId(status.Id)
                .BuildAsync();

            var comment = new ActivityComment
            {
                ActivityId = activity.Id,
                Value = "Test".WithGuid()
            };

            await _activityCommentsClient.CreateAsync(accessToken, comment);

            var request = new ActivityCommentGetPagedListRequest
            {
                ActivityId = activity.Id
            };

            var createdComment = (await _activityCommentsClient.GetPagedListAsync(accessToken, request)).Comments
                .First();

            Assert.NotNull(createdComment);
            Assert.Equal(comment.ActivityId, createdComment.ActivityId);
            Assert.True(!createdComment.CommentatorUserId.IsEmpty());
            Assert.Equal(comment.Value, createdComment.Value);
            Assert.True(createdComment.CreateDateTime.IsMoreThanMinValue());
        }
    }
}
