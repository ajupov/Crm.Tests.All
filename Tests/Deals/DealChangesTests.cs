using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.V1.Clients.Deals.Clients;
using Crm.V1.Clients.Deals.Models;
using Crm.V1.Clients.Deals.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Deals
{
    public class DealChangesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IDealsClient _dealsClient;
        private readonly IDealChangesClient _dealChangesClient;

        public DealChangesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IDealsClient dealsClient,
            IDealChangesClient dealChangesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _dealsClient = dealsClient;
            _dealChangesClient = dealChangesClient;
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

            deal.IsDeleted = true;

            await _dealsClient.UpdateAsync(accessToken, deal);

            var request = new DealChangeGetPagedListRequest
            {
                DealId = deal.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _dealChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.DealId == deal.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<Deal>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<Deal>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<Deal>().IsDeleted);
        }
    }
}
