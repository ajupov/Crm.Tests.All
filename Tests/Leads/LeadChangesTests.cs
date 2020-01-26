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
using Crm.v1.Clients.Leads.RequestParameters;
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

            var request = new LeadChangeGetPagedListRequestParameter
            {
                LeadId = lead.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var changes = await _leadChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(changes);
            Assert.True(changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(changes.All(x => x.LeadId == lead.Id));
            Assert.True(changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(changes.First().OldValueJson.IsEmpty());
            Assert.True(!changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(changes.First().NewValueJson.FromJsonString<Lead>());
            Assert.True(!changes.Last().OldValueJson.IsEmpty());
            Assert.True(!changes.Last().NewValueJson.IsEmpty());
            Assert.False(changes.Last().OldValueJson.FromJsonString<Lead>().IsDeleted);
            Assert.True(changes.Last().NewValueJson.FromJsonString<Lead>().IsDeleted);
        }
    }
}