using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Deals.Clients;
using Crm.v1.Clients.Deals.Models;
using Crm.v1.Clients.Deals.RequestParameters;
using Xunit;

namespace Crm.Tests.All.Tests.Deals
{
    public class DealStatusChangesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IDealStatusesClient _dealStatusesClient;
        private readonly IDealStatusChangesClient _statusChangesClient;

        public DealStatusChangesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IDealStatusesClient dealStatusesClient,
            IDealStatusChangesClient statusChangesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _dealStatusesClient = dealStatusesClient;
            _statusChangesClient = statusChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var status = await _create.DealStatus.BuildAsync();

            status.Name = "Test".WithGuid();
            status.IsDeleted = true;

            await _dealStatusesClient.UpdateAsync(accessToken, status);

            var request = new DealStatusChangeGetPagedListRequestParameter
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
            Assert.NotNull(changes.First().NewValueJson.FromJsonString<DealStatus>());
            Assert.True(!changes.Last().OldValueJson.IsEmpty());
            Assert.True(!changes.Last().NewValueJson.IsEmpty());
            Assert.False(changes.Last().OldValueJson.FromJsonString<DealStatus>().IsDeleted);
            Assert.True(changes.Last().NewValueJson.FromJsonString<DealStatus>().IsDeleted);
            Assert.Equal(changes.Last().NewValueJson.FromJsonString<DealStatus>().Name, status.Name);
        }
    }
}