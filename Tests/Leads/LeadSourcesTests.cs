using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Leads.Clients;
using Crm.v1.Clients.Leads.Models;
using Crm.v1.Clients.Leads.RequestParameters;
using Xunit;

namespace Crm.Tests.All.Tests.Leads
{
    public class LeadSourcesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly ILeadSourcesClient _leadSourcesClient;

        public LeadSourcesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            ILeadSourcesClient leadSourcesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _leadSourcesClient = leadSourcesClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var statusId = (await _create.LeadSource.BuildAsync()).Id;

            var status = await _leadSourcesClient.GetAsync(accessToken, statusId);

            Assert.NotNull(status);
            Assert.Equal(statusId, status.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var statusIds = (
                    await Task.WhenAll(
                        _create.LeadSource
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.LeadSource
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var statuses = await _leadSourcesClient.GetListAsync(accessToken, statusIds);

            Assert.NotEmpty(statuses);
            Assert.Equal(statusIds.Count, statuses.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var name = "Test".WithGuid();
            await Task.WhenAll(
                _create.LeadSource
                    .WithName(name)
                    .BuildAsync());

            var request = new LeadSourceGetPagedListRequestParameter
            {
                Name = name
            };

            var statuses = await _leadSourcesClient.GetPagedListAsync(accessToken, request);

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

            var status = new LeadSource
            {
                Name = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdSourceId = await _leadSourcesClient.CreateAsync(accessToken, status);

            var createdSource = await _leadSourcesClient.GetAsync(accessToken, createdSourceId);

            Assert.NotNull(createdSource);
            Assert.Equal(createdSourceId, createdSource.Id);
            Assert.Equal(status.Name, createdSource.Name);
            Assert.Equal(status.IsDeleted, createdSource.IsDeleted);
            Assert.True(createdSource.CreateDateTime.IsMoreThanMinValue());
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var status = await _create.LeadSource
                .WithName("Test".WithGuid())
                .BuildAsync();

            status.Name = "Test".WithGuid();
            status.IsDeleted = true;

            await _leadSourcesClient.UpdateAsync(accessToken, status);

            var updatedSource = await _leadSourcesClient.GetAsync(accessToken, status.Id);

            Assert.Equal(status.Name, updatedSource.Name);
            Assert.Equal(status.IsDeleted, updatedSource.IsDeleted);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var statusIds = (
                    await Task.WhenAll(
                        _create.LeadSource
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.LeadSource
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _leadSourcesClient.DeleteAsync(accessToken, statusIds);

            var statuses = await _leadSourcesClient.GetListAsync(accessToken, statusIds);

            Assert.All(statuses, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var statusIds = (
                    await Task.WhenAll(
                        _create.LeadSource
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.LeadSource
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _leadSourcesClient.RestoreAsync(accessToken, statusIds);

            var statuses = await _leadSourcesClient.GetListAsync(accessToken, statusIds);

            Assert.All(statuses, x => Assert.False(x.IsDeleted));
        }
    }
}