using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.V1.Clients.Deals.Clients;
using Crm.V1.Clients.Deals.Models;
using Crm.V1.Clients.Deals.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Deals
{
    public class DealTypesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IDealTypesClient _dealTypesClient;

        public DealTypesTests(IAccessTokenGetter accessTokenGetter, ICreate create, IDealTypesClient dealTypesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _dealTypesClient = dealTypesClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var typeId = (await _create.DealType.BuildAsync()).Id;

            var type = await _dealTypesClient.GetAsync(accessToken, typeId);

            Assert.NotNull(type);
            Assert.Equal(typeId, type.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var typeIds = (
                    await Task.WhenAll(
                        _create.DealType
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.DealType
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var types = await _dealTypesClient.GetListAsync(accessToken, typeIds);

            Assert.NotEmpty(types);
            Assert.Equal(typeIds.Count, types.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var name = "Test".WithGuid();
            await Task.WhenAll(
                _create.DealType
                    .WithName(name)
                    .BuildAsync());

            var request = new DealTypeGetPagedListRequest
            {
                Name = name
            };

            var response = await _dealTypesClient.GetPagedListAsync(accessToken, request);

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

            var type = new DealType
            {
                Name = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdTypeId = await _dealTypesClient.CreateAsync(accessToken, type);

            var createdType = await _dealTypesClient.GetAsync(accessToken, createdTypeId);

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

            var type = await _create.DealType
                .WithName("Test".WithGuid())
                .BuildAsync();

            type.Name = "Test".WithGuid();
            type.IsDeleted = true;

            await _dealTypesClient.UpdateAsync(accessToken, type);

            var updatedType = await _dealTypesClient.GetAsync(accessToken, type.Id);

            Assert.Equal(type.Name, updatedType.Name);
            Assert.Equal(type.IsDeleted, updatedType.IsDeleted);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var typeIds = (
                    await Task.WhenAll(
                        _create.DealType
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.DealType
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _dealTypesClient.DeleteAsync(accessToken, typeIds);

            var types = await _dealTypesClient.GetListAsync(accessToken, typeIds);

            Assert.All(types, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var typeIds = (
                    await Task.WhenAll(
                        _create.DealType
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.DealType
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _dealTypesClient.RestoreAsync(accessToken, typeIds);

            var types = await _dealTypesClient.GetListAsync(accessToken, typeIds);

            Assert.All(types, x => Assert.False(x.IsDeleted));
        }
    }
}
