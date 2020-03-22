using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Leads.Clients;
using Crm.v1.Clients.Leads.Models;
using Crm.v1.Clients.Leads.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Leads
{
    public class LeadChangesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly ILeadsClient _leadsClient;
        private readonly ILeadChangesClient _leadChangesClient;

        public LeadChangesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            ILeadsClient leadsClient,
            ILeadChangesClient leadChangesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _leadsClient = leadsClient;
            _leadChangesClient = leadChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var source = await _create.LeadSource.BuildAsync();
            var lead = await _create.Lead
                .WithSourceId(source.Id)
                .BuildAsync();

            lead.IsDeleted = true;

            await _leadsClient.UpdateAsync(accessToken, lead);

            var request = new LeadChangeGetPagedListRequest
            {
                LeadId = lead.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _leadChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.LeadId == lead.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<Lead>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<Lead>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<Lead>().IsDeleted);
        }
    }
}