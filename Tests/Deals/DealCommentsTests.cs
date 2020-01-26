using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Deals.Clients;
using Crm.v1.Clients.Deals.Models;
using Crm.v1.Clients.Deals.RequestParameters;
using Xunit;

namespace Crm.Tests.All.Tests.Deals
{
    public class DealCommentsTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IDealCommentsClient _dealCommentsClient;

        public DealCommentsTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IDealCommentsClient dealCommentsClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _dealCommentsClient = dealCommentsClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.DealType.BuildAsync();
            var status = await _create.DealStatus.BuildAsync();
            var deal = await _create.Deal
                .WithTypeId(type.Id)
                .WithStatusId(status.Id)
                .BuildAsync();
            await Task.WhenAll(
                _create.DealComment
                    .WithDealId(deal.Id)
                    .BuildAsync(),
                _create.DealComment
                    .WithDealId(deal.Id)
                    .BuildAsync());

            var request = new DealCommentGetPagedListRequestParameter
            {
                DealId = deal.Id,
            };

            var comments = await _dealCommentsClient.GetPagedListAsync(accessToken, request);

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

            var type = await _create.DealType.BuildAsync();
            var status = await _create.DealStatus.BuildAsync();
            var deal = await _create.Deal
                .WithTypeId(type.Id)
                .WithStatusId(status.Id)
                .BuildAsync();

            var comment = new DealComment
            {
                DealId = deal.Id,
                Value = "Test".WithGuid()
            };

            await _dealCommentsClient.CreateAsync(accessToken, comment);

            var request = new DealCommentGetPagedListRequestParameter
            {
                DealId = deal.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var createdComment = (await _dealCommentsClient.GetPagedListAsync(accessToken, request)).First();

            Assert.NotNull(createdComment);
            Assert.Equal(comment.DealId, createdComment.DealId);
            Assert.True(!createdComment.CommentatorUserId.IsEmpty());
            Assert.Equal(comment.Value, createdComment.Value);
            Assert.True(createdComment.CreateDateTime.IsMoreThanMinValue());
        }
    }
}