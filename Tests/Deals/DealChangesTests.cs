using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Deals.Clients;
using Crm.v1.Clients.Deals.Models;
using Crm.v1.Clients.Deals.RequestParameters;
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

            var request = new DealChangeGetPagedListRequestParameter
            {
                DealId = deal.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var changes = await _dealChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(changes);
            Assert.True(changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(changes.All(x => x.DealId == deal.Id));
            Assert.True(changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(changes.First().OldValueJson.IsEmpty());
            Assert.True(!changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(changes.First().NewValueJson.FromJsonString<Deal>());
            Assert.True(!changes.Last().OldValueJson.IsEmpty());
            Assert.True(!changes.Last().NewValueJson.IsEmpty());
            Assert.False(changes.Last().OldValueJson.FromJsonString<Deal>().IsDeleted);
            Assert.True(changes.Last().NewValueJson.FromJsonString<Deal>().IsDeleted);
        }
    }
}