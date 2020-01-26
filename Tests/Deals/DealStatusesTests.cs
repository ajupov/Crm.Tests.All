using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Deals.Clients;
using Crm.v1.Clients.Deals.Models;
using Crm.v1.Clients.Deals.RequestParameters;
using Xunit;

namespace Crm.Tests.All.Tests.Deals
{
    public class DealStatusesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IDealStatusesClient _dealStatusesClient;

        public DealStatusesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IDealStatusesClient dealStatusesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _dealStatusesClient = dealStatusesClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var statusId = (await _create.DealStatus.BuildAsync()).Id;

            var status = await _dealStatusesClient.GetAsync(accessToken, statusId);

            Assert.NotNull(status);
            Assert.Equal(statusId, status.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var statusIds = (
                    await Task.WhenAll(
                        _create.DealStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.DealStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var statuses = await _dealStatusesClient.GetListAsync(accessToken, statusIds);

            Assert.NotEmpty(statuses);
            Assert.Equal(statusIds.Count, statuses.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var name = "Test".WithGuid();
            await Task.WhenAll(
                _create.DealStatus
                    .WithName(name)
                    .BuildAsync());

            var request = new DealStatusGetPagedListRequestParameter
            {
                Name = name
            };

            var statuses = await _dealStatusesClient.GetPagedListAsync(accessToken, request);

            var results = statuses
                .Skip(1)
                .Zip(statuses, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(statuses);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var status = new DealStatus
            {
                Name = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdStatusId = await _dealStatusesClient.CreateAsync(accessToken, status);

            var createdStatus = await _dealStatusesClient.GetAsync(accessToken, createdStatusId);

            Assert.NotNull(createdStatus);
            Assert.Equal(createdStatusId, createdStatus.Id);
            Assert.Equal(status.Name, createdStatus.Name);
            Assert.Equal(status.IsDeleted, createdStatus.IsDeleted);
            Assert.True(createdStatus.CreateDateTime.IsMoreThanMinValue());
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var status = await _create.DealStatus
                .WithName("Test".WithGuid())
                .BuildAsync();

            status.Name = "Test".WithGuid();
            status.IsDeleted = true;

            await _dealStatusesClient.UpdateAsync(accessToken, status);

            var updatedStatus = await _dealStatusesClient.GetAsync(accessToken, status.Id);

            Assert.Equal(status.Name, updatedStatus.Name);
            Assert.Equal(status.IsDeleted, updatedStatus.IsDeleted);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var statusIds = (
                    await Task.WhenAll(
                        _create.DealStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.DealStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _dealStatusesClient.DeleteAsync(accessToken, statusIds);

            var statuses = await _dealStatusesClient.GetListAsync(accessToken, statusIds);

            Assert.All(statuses, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var statusIds = (
                    await Task.WhenAll(
                        _create.DealStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.DealStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _dealStatusesClient.RestoreAsync(accessToken, statusIds);

            var statuses = await _dealStatusesClient.GetListAsync(accessToken, statusIds);

            Assert.All(statuses, x => Assert.False(x.IsDeleted));
        }
    }
}