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
using Crm.v1.Clients.Leads.RequestParameters;
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

            var request = new LeadSourceChangeGetPagedListRequestParameter
            {
                SourceId = source.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var changes = await _sourceChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(changes);
            Assert.True(changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(changes.All(x => x.SourceId == source.Id));
            Assert.True(changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(changes.First().OldValueJson.IsEmpty());
            Assert.True(!changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(changes.First().NewValueJson.FromJsonString<LeadSource>());
            Assert.True(!changes.Last().OldValueJson.IsEmpty());
            Assert.True(!changes.Last().NewValueJson.IsEmpty());
            Assert.False(changes.Last().OldValueJson.FromJsonString<LeadSource>().IsDeleted);
            Assert.True(changes.Last().NewValueJson.FromJsonString<LeadSource>().IsDeleted);
            Assert.Equal(changes.Last().NewValueJson.FromJsonString<LeadSource>().Name, source.Name);
        }
    }
}