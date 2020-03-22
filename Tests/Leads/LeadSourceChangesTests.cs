using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Leads.Clients;
using Crm.v1.Clients.Leads.Models;
using Crm.v1.Clients.Leads.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Leads
{
    public class LeadSourceChangesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly ILeadSourcesClient _leadSourcesClient;
        private readonly ILeadSourceChangesClient _sourceChangesClient;

        public LeadSourceChangesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            ILeadSourcesClient leadSourcesClient,
            ILeadSourceChangesClient sourceChangesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _leadSourcesClient = leadSourcesClient;
            _sourceChangesClient = sourceChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var source = await _create.LeadSource.BuildAsync();

            source.Name = "Test".WithGuid();
            source.IsDeleted = true;

            await _leadSourcesClient.UpdateAsync(accessToken, source);

            var request = new LeadSourceChangeGetPagedListRequest
            {
                SourceId = source.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _sourceChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.SourceId == source.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<LeadSource>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<LeadSource>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<LeadSource>().IsDeleted);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<LeadSource>().Name, source.Name);
        }
    }
}