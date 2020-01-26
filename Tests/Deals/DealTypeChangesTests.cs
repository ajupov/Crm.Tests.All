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

            var request = new DealTypeChangeGetPagedListRequestParameter
            {
                TypeId = type.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var changes = await _typeChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(changes);
            Assert.True(changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(changes.All(x => x.TypeId == type.Id));
            Assert.True(changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(changes.First().OldValueJson.IsEmpty());
            Assert.True(!changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(changes.First().NewValueJson.FromJsonString<DealType>());
            Assert.True(!changes.Last().OldValueJson.IsEmpty());
            Assert.True(!changes.Last().NewValueJson.IsEmpty());
            Assert.False(changes.Last().OldValueJson.FromJsonString<DealType>().IsDeleted);
            Assert.True(changes.Last().NewValueJson.FromJsonString<DealType>().IsDeleted);
            Assert.Equal(changes.Last().NewValueJson.FromJsonString<DealType>().Name, type.Name);
        }
    }
}