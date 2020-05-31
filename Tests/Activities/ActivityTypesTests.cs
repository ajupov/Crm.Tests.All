using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.V1.Clients.Activities.Clients;
using Crm.V1.Clients.Activities.Models;
using Crm.V1.Clients.Activities.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Activities
{
    public class ActivityTypesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IActivityTypesClient _activityTypesClient;

        public ActivityTypesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IActivityTypesClient activityTypesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _activityTypesClient = activityTypesClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var typeId = (await _create.ActivityType.BuildAsync()).Id;

            var type = await _activityTypesClient.GetAsync(accessToken, typeId);

            Assert.NotNull(type);
            Assert.Equal(typeId, type.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var typeIds = (
                    await Task.WhenAll(
                        _create.ActivityType
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.ActivityType
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var types = await _activityTypesClient.GetListAsync(accessToken, typeIds);

            Assert.NotEmpty(types);
            Assert.Equal(typeIds.Count, types.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var name = "Test".WithGuid();
            await Task.WhenAll(_create.ActivityType.WithName(name).BuildAsync());

            var request = new ActivityTypeGetPagedListRequest
            {
                Name = name,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _activityTypesClient.GetPagedListAsync(accessToken, request);

            var results = response.Types
                .Skip(1)
                .Zip(response.Types, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Types);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = new ActivityType
            {
                Name = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdTypeId = await _activityTypesClient.CreateAsync(accessToken, type);

            var createdType = await _activityTypesClient.GetAsync(accessToken, createdTypeId);

            Assert.NotNull(createdType);
            Assert.Equal(createdTypeId, createdType.Id);
            Assert.Equal(type.Name, createdType.Name);
            Assert.Equal(type.IsDeleted, createdType.IsDeleted);
            Assert.True(createdType.CreateDateTime.IsMoreThanMinValue());
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.ActivityType.WithName("Test".WithGuid()).BuildAsync();

            type.Name = "Test".WithGuid();
            type.IsDeleted = true;

            await _activityTypesClient.UpdateAsync(accessToken, type);

            var updatedType = await _activityTypesClient.GetAsync(accessToken, type.Id);

            Assert.Equal(type.Name, updatedType.Name);
            Assert.Equal(type.IsDeleted, updatedType.IsDeleted);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var typeIds = (await Task.WhenAll(
                    _create.ActivityType.WithName("Test".WithGuid()).BuildAsync(),
                    _create.ActivityType.WithName("Test".WithGuid()).BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _activityTypesClient.DeleteAsync(accessToken, typeIds);

            var types = await _activityTypesClient.GetListAsync(accessToken, typeIds);

            Assert.All(types, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var typeIds = (
                    await Task.WhenAll(
                        _create.ActivityType
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.ActivityType
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _activityTypesClient.RestoreAsync(accessToken, typeIds);

            var types = await _activityTypesClient.GetListAsync(accessToken, typeIds);

            Assert.All(types, x => Assert.False(x.IsDeleted));
        }
    }
}
