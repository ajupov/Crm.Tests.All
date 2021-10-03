using System;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Customers.Clients;
using Crm.v1.Clients.Customers.Models;
using Xunit;

namespace Crm.Tests.All.Tests.Customers
{
    public class CustomerSourcesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ICustomerSourcesClient _customerSourcesClient;

        public CustomerSourcesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ICustomerSourcesClient customerSourcesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _customerSourcesClient = customerSourcesClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var statusId = (await _create.CustomerSource.BuildAsync()).Id;

            var status = await _customerSourcesClient.GetAsync(statusId, headers);

            Assert.NotNull(status);
            Assert.Equal(statusId, status.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var statusIds = (
                    await Task.WhenAll(
                        _create.CustomerSource
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.CustomerSource
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var statuses = await _customerSourcesClient.GetListAsync(statusIds, headers);

            Assert.NotEmpty(statuses);
            Assert.Equal(statusIds.Count, statuses.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var name = "Test".WithGuid();
            await Task.WhenAll(
                _create.CustomerSource
                    .WithName(name)
                    .BuildAsync());

            var request = new CustomerSourceGetPagedListRequest
            {
                Name = name
            };

            var response = await _customerSourcesClient.GetPagedListAsync(request, headers);

            var results = response.Sources
                .Skip(1)
                .Zip(response.Sources, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Sources);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = new CustomerSource
            {
                Id = Guid.NewGuid(),
                Name = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdSourceId = await _customerSourcesClient.CreateAsync(status, headers);

            var createdSource = await _customerSourcesClient.GetAsync(createdSourceId, headers);

            Assert.NotNull(createdSource);
            Assert.Equal(createdSourceId, createdSource.Id);
            Assert.Equal(status.Name, createdSource.Name);
            Assert.Equal(status.IsDeleted, createdSource.IsDeleted);
            Assert.True(createdSource.CreateDateTime.IsMoreThanMinValue());
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = await _create.CustomerSource
                .WithName("Test".WithGuid())
                .BuildAsync();

            status.Name = "Test".WithGuid();
            status.IsDeleted = true;

            await _customerSourcesClient.UpdateAsync(status, headers);

            var updatedSource = await _customerSourcesClient.GetAsync(status.Id, headers);

            Assert.Equal(status.Name, updatedSource.Name);
            Assert.Equal(status.IsDeleted, updatedSource.IsDeleted);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var statusIds = (
                    await Task.WhenAll(
                        _create.CustomerSource
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.CustomerSource
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _customerSourcesClient.DeleteAsync(statusIds, headers);

            var statuses = await _customerSourcesClient.GetListAsync(statusIds, headers);

            Assert.All(statuses, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var statusIds = (
                    await Task.WhenAll(
                        _create.CustomerSource
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.CustomerSource
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _customerSourcesClient.RestoreAsync(statusIds, headers);

            var statuses = await _customerSourcesClient.GetListAsync(statusIds, headers);

            Assert.All(statuses, x => Assert.False(x.IsDeleted));
        }
    }
}
