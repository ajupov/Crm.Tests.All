using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Activities.Clients;
using Crm.v1.Clients.Activities.Models;
using Crm.v1.Clients.Activities.RequestParameters;
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

            var request = new ActivityCommentGetPagedListRequestParameter
            {
                ActivityId = activity.Id
            };

            var comments = await _activityCommentsClient.GetPagedListAsync(accessToken, request);

            var results = comments
                .Skip(1)
                .Zip(comments, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(comments);
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

            var request = new ActivityCommentGetPagedListRequestParameter
            {
                ActivityId = activity.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var createdComment = (await _activityCommentsClient.GetPagedListAsync(accessToken, request)).First();

            Assert.NotNull(createdComment);
            Assert.Equal(comment.ActivityId, createdComment.ActivityId);
            Assert.True(!createdComment.CommentatorUserId.IsEmpty());
            Assert.Equal(comment.Value, createdComment.Value);
            Assert.True(createdComment.CreateDateTime.IsMoreThanMinValue());
        }
    }
}