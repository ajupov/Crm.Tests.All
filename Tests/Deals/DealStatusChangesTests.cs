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
using Crm.v1.Clients.Deals.Requests;
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

            var request = new DealStatusChangeGetPagedListRequest
            {
                StatusId = status.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _statusChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.StatusId == status.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<DealStatus>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<DealStatus>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<DealStatus>().IsDeleted);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<DealStatus>().Name, status.Name);
        }
    }
}