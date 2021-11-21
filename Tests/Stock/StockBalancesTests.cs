using System;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Stock.Clients;
using Crm.v1.Clients.Stock.Models;
using Xunit;

namespace Crm.Tests.All.Tests.Stock
{
    public class StockBalancesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IStockBalancesClient _stockBalancesClient;

        public StockBalancesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IStockBalancesClient stockBalancesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _stockBalancesClient = stockBalancesClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var room = await _create.StockRoom.BuildAsync();
            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var balanceId = (
                    await _create.StockBalance
                        .WithRoomId(room.Id)
                        .WithProductId(product.Id)
                        .WithCount(1)
                        .BuildAsync())
                .Id;

            var balance = await _stockBalancesClient.GetAsync(balanceId, headers);

            Assert.NotNull(balance);
            Assert.Equal(balanceId, balance.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var room = await _create.StockRoom.BuildAsync();
            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var balanceIds = (
                    await Task.WhenAll(
                        _create.StockBalance
                            .WithRoomId(room.Id)
                            .WithProductId(product.Id)
                            .WithCount(1)
                            .BuildAsync(),
                        _create.StockBalance
                            .WithRoomId(room.Id)
                            .WithProductId(product.Id)
                            .WithCount(1)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var balances = await _stockBalancesClient.GetListAsync(balanceIds, headers);

            Assert.NotEmpty(balances);
            Assert.Equal(balanceIds.Count, balances.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var room = await _create.StockRoom.BuildAsync();
            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            await Task.WhenAll(
                _create.StockBalance
                    .WithRoomId(room.Id)
                    .WithProductId(product.Id)
                    .WithCount(1)
                    .BuildAsync(),
                _create.StockBalance
                    .WithRoomId(room.Id)
                    .WithProductId(product.Id)
                    .WithCount(1)
                    .BuildAsync());

            var request = new StockBalanceGetPagedListRequest();

            var response = await _stockBalancesClient.GetPagedListAsync(request, headers);

            var results = response.Balances
                .Skip(1)
                .Zip(response.Balances, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Balances);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var room = await _create.StockRoom.BuildAsync();
            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var balance = new StockBalance
            {
                Id = Guid.NewGuid(),
                RoomId = room.Id,
                ProductId = product.Id,
                Count = 1,
                IsDeleted = true
            };

            var createdBalanceId = await _stockBalancesClient.CreateAsync(balance, headers);

            var createdBalance = await _stockBalancesClient.GetAsync(createdBalanceId, headers);

            Assert.NotNull(createdBalance);
            Assert.Equal(createdBalanceId, createdBalance.Id);
            Assert.Equal(balance.RoomId, createdBalance.RoomId);
            Assert.Equal(balance.ProductId, createdBalance.ProductId);
            Assert.Equal(balance.Count, createdBalance.Count);
            Assert.True(!createdBalance.CreateUserId.IsEmpty());
            Assert.Equal(balance.IsDeleted, createdBalance.IsDeleted);
            Assert.True(createdBalance.CreateDateTime.IsMoreThanMinValue());
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var room = await _create.StockRoom.BuildAsync();
            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var balance = await _create.StockBalance
                .WithRoomId(room.Id)
                .WithProductId(product.Id)
                .WithCount(1)
                .BuildAsync();

            balance.Count = 2;
            balance.IsDeleted = true;

            await _stockBalancesClient.UpdateAsync(balance, headers);

            var updatedBalance = await _stockBalancesClient.GetAsync(balance.Id, headers);

            Assert.Equal(balance.AccountId, updatedBalance.AccountId);
            Assert.Equal(balance.CreateUserId, updatedBalance.CreateUserId);
            Assert.Equal(balance.RoomId, updatedBalance.RoomId);
            Assert.Equal(balance.ProductId, updatedBalance.ProductId);
            Assert.Equal(balance.Count, updatedBalance.Count);
            Assert.Equal(balance.IsDeleted, updatedBalance.IsDeleted);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var room = await _create.StockRoom.BuildAsync();
            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var balanceIds = (
                    await Task.WhenAll(
                        _create.StockBalance
                            .WithRoomId(room.Id)
                            .WithProductId(product.Id)
                            .WithCount(1)
                            .BuildAsync(),
                        _create.StockBalance
                            .WithRoomId(room.Id)
                            .WithProductId(product.Id)
                            .WithCount(1)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _stockBalancesClient.DeleteAsync(balanceIds, headers);

            var balances = await _stockBalancesClient.GetListAsync(balanceIds, headers);

            Assert.All(balances, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var room = await _create.StockRoom.BuildAsync();
            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var balanceIds = (
                    await Task.WhenAll(
                        _create.StockBalance
                            .WithRoomId(room.Id)
                            .WithProductId(product.Id)
                            .WithCount(1)
                            .BuildAsync(),
                        _create.StockBalance
                            .WithRoomId(room.Id)
                            .WithProductId(product.Id)
                            .WithCount(1)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _stockBalancesClient.RestoreAsync(balanceIds, headers);

            var balances = await _stockBalancesClient.GetListAsync(balanceIds, headers);

            Assert.All(balances, x => Assert.False(x.IsDeleted));
        }
    }
}
