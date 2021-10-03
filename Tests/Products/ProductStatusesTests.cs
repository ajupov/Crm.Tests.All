using System;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;
using Xunit;

namespace Crm.Tests.All.Tests.Products
{
    public class ProductStatusesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IProductStatusesClient _productStatusesClient;

        public ProductStatusesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IProductStatusesClient productStatusesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _productStatusesClient = productStatusesClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var statusId = (await _create.ProductStatus.BuildAsync()).Id;

            var status = await _productStatusesClient.GetAsync(statusId, headers);

            Assert.NotNull(status);
            Assert.Equal(statusId, status.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var statusIds = (
                    await Task.WhenAll(
                        _create.ProductStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.ProductStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var status = await _productStatusesClient.GetListAsync(statusIds, headers);

            Assert.NotEmpty(status);
            Assert.Equal(statusIds.Count, status.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var name = "Test".WithGuid();
            await Task.WhenAll(
                _create.ProductStatus
                    .WithName(name)
                    .BuildAsync());

            var request = new ProductStatusGetPagedListRequest
            {
                Name = name
            };

            var response = await _productStatusesClient.GetPagedListAsync(request, headers);

            var results = response.Statuses
                .Skip(1)
                .Zip(response.Statuses, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Statuses);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = new ProductStatus
            {
                Id = Guid.NewGuid(),
                Name = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdStatusId = await _productStatusesClient.CreateAsync(status, headers);

            var createdStatus = await _productStatusesClient.GetAsync(createdStatusId, headers);

            Assert.NotNull(createdStatus);
            Assert.Equal(createdStatusId, createdStatus.Id);
            Assert.Equal(status.Name, createdStatus.Name);
            Assert.Equal(status.IsDeleted, createdStatus.IsDeleted);
            Assert.True(createdStatus.CreateDateTime.IsMoreThanMinValue());
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = await _create.ProductStatus
                .WithName("Test".WithGuid())
                .BuildAsync();

            status.Name = "Test".WithGuid();
            status.IsDeleted = true;

            await _productStatusesClient.UpdateAsync(status, headers);

            var updatedStatus = await _productStatusesClient.GetAsync(status.Id, headers);

            Assert.Equal(status.Name, updatedStatus.Name);
            Assert.Equal(status.IsDeleted, updatedStatus.IsDeleted);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var statusIds = (
                    await Task.WhenAll(
                        _create.ProductStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.ProductStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _productStatusesClient.DeleteAsync(statusIds, headers);

            var statuses = await _productStatusesClient.GetListAsync(statusIds, headers);

            Assert.All(statuses, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var statusIds = (
                    await Task.WhenAll(
                        _create.ProductStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.ProductStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _productStatusesClient.RestoreAsync(statusIds, headers);

            var statuses = await _productStatusesClient.GetListAsync(statusIds, headers);

            Assert.All(statuses, x => Assert.False(x.IsDeleted));
        }
    }
}
