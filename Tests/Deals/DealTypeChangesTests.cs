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
    public class DealTypeChangesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IDealTypesClient _dealTypesClient;
        private readonly IDealTypeChangesClient _typeChangesClient;

        public DealTypeChangesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IDealTypesClient dealTypesClient,
            IDealTypeChangesClient typeChangesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _dealTypesClient = dealTypesClient;
            _typeChangesClient = typeChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.DealType.BuildAsync();

            type.Name = "Test".WithGuid();
            type.IsDeleted = true;

            await _dealTypesClient.UpdateAsync(accessToken, type);

            var request = new DealTypeChangeGetPagedListRequest
            {
                TypeId = type.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _typeChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.TypeId == type.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<DealType>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<DealType>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<DealType>().IsDeleted);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<DealType>().Name, type.Name);
        }
    }
}