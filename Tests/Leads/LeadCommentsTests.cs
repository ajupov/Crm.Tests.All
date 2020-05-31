using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.V1.Clients.Leads.Clients;
using Crm.V1.Clients.Leads.Models;
using Crm.V1.Clients.Leads.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Leads
{
    public class LeadCommentsTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly ILeadCommentsClient _leadCommentsClient;

        public LeadCommentsTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            ILeadCommentsClient leadCommentsClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _leadCommentsClient = leadCommentsClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var source = await _create.LeadSource.BuildAsync();
            var lead = await _create.Lead
                .WithSourceId(source.Id)
                .BuildAsync();
            await Task.WhenAll(
                _create.LeadComment
                    .WithLeadId(lead.Id)
                    .BuildAsync(),
                _create.LeadComment
                    .WithLeadId(lead.Id)
                    .BuildAsync());

            var request = new LeadCommentGetPagedListRequest
            {
                LeadId = lead.Id
            };

            var response = await _leadCommentsClient.GetPagedListAsync(accessToken, request);

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

            var source = await _create.LeadSource.BuildAsync();
            var lead = await _create.Lead
                .WithSourceId(source.Id)
                .BuildAsync();

            var comment = new LeadComment
            {
                LeadId = lead.Id,
                Value = "Test".WithGuid()
            };

            await _leadCommentsClient.CreateAsync(accessToken, comment);

            var request = new LeadCommentGetPagedListRequest
            {
                LeadId = lead.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var createdComment = (await _leadCommentsClient.GetPagedListAsync(accessToken, request)).Comments.First();

            Assert.NotNull(createdComment);
            Assert.Equal(comment.LeadId, createdComment.LeadId);
            Assert.True(!createdComment.CommentatorUserId.IsEmpty());
            Assert.Equal(comment.Value, createdComment.Value);
            Assert.True(createdComment.CreateDateTime.IsMoreThanMinValue());
        }
    }
}
