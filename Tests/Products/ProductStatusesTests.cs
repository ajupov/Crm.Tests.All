using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;
using Crm.v1.Clients.Products.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Products
{
    public class ProductStatusesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IProductStatusesClient _productStatusesClient;

        public ProductStatusesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IProductStatusesClient productStatusesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _productStatusesClient = productStatusesClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var statusId = (await _create.ProductStatus.BuildAsync()).Id;

            var status = await _productStatusesClient.GetAsync(accessToken, statusId);

            Assert.NotNull(status);
            Assert.Equal(statusId, status.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

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

            var status = await _productStatusesClient.GetListAsync(accessToken, statusIds);

            Assert.NotEmpty(status);
            Assert.Equal(statusIds.Count, status.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var name = "Test".WithGuid();
            await Task.WhenAll(
                _create.ProductStatus
                    .WithName(name)
                    .BuildAsync());

            var request = new ProductStatusGetPagedListRequest
            {
                Name = name
            };

            var response = await _productStatusesClient.GetPagedListAsync(accessToken, request);

            var results = response.Statuses
                .Skip(1)
                .Zip(response.Statuses, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Statuses);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var status = new ProductStatus
            {
                Name = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdStatusId = await _productStatusesClient.CreateAsync(accessToken, status);

            var createdStatus = await _productStatusesClient.GetAsync(accessToken, createdStatusId);

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

            var status = await _create.ProductStatus
                .WithName("Test".WithGuid())
                .BuildAsync();

            status.Name = "Test".WithGuid();
            status.IsDeleted = true;

            await _productStatusesClient.UpdateAsync(accessToken, status);

            var updatedStatus = await _productStatusesClient.GetAsync(accessToken, status.Id);

            Assert.Equal(status.Name, updatedStatus.Name);
            Assert.Equal(status.IsDeleted, updatedStatus.IsDeleted);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

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

            await _productStatusesClient.DeleteAsync(accessToken, statusIds);

            var statuses = await _productStatusesClient.GetListAsync(accessToken, statusIds);

            Assert.All(statuses, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

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

            await _productStatusesClient.RestoreAsync(accessToken, statusIds);

            var statuses = await _productStatusesClient.GetListAsync(accessToken, statusIds);

            Assert.All(statuses, x => Assert.False(x.IsDeleted));
        }
    }
}