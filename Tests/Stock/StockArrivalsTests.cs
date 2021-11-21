using System;
using System.Collections.Generic;
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
    public class StockArrivalsTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IStockArrivalsClient _stockArrivalsClient;

        public StockArrivalsTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IStockArrivalsClient stockArrivalsClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _stockArrivalsClient = stockArrivalsClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var arrivalId = (
                    await _create.StockArrival
                        .WithType(StockArrivalType.ArrivalFromSupplier)
                        .WithItem(product.Id, 1)
                        .BuildAsync())
                .Id;

            var arrival = await _stockArrivalsClient.GetAsync(arrivalId, headers);

            Assert.NotNull(arrival);
            Assert.Equal(arrivalId, arrival.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var arrivalIds = (
                    await Task.WhenAll(
                        _create.StockArrival
                            .WithType(StockArrivalType.ArrivalFromSupplier)
                            .WithItem(product.Id, 1)
                            .BuildAsync(),
                        _create.StockArrival
                            .WithType(StockArrivalType.ArrivalFromSupplier)
                            .WithItem(product.Id, 1)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var arrivals = await _stockArrivalsClient.GetListAsync(arrivalIds, headers);

            Assert.NotEmpty(arrivals);
            Assert.Equal(arrivalIds.Count, arrivals.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            await Task.WhenAll(
                _create.StockArrival
                    .WithType(StockArrivalType.ArrivalFromSupplier)
                    .WithItem(product.Id, 1)
                    .BuildAsync(),
                _create.StockArrival
                    .WithType(StockArrivalType.ArrivalFromSupplier)
                    .WithItem(product.Id, 1)
                    .BuildAsync());

            var request = new StockArrivalGetPagedListRequest();

            var response = await _stockArrivalsClient.GetPagedListAsync(request, headers);

            var results = response.Arrivals
                .Skip(1)
                .Zip(response.Arrivals, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Arrivals);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var arrival = new StockArrival
            {
                Id = Guid.NewGuid(),
                Type = StockArrivalType.ArrivalFromSupplier,
                IsDeleted = true,
                Items = new List<StockArrivalItem>
                {
                    new ()
                    {
                        ProductId = product.Id,
                        Count = 1
                    }
                },
            };

            var createdArrivalId = await _stockArrivalsClient.CreateAsync(arrival, headers);

            var createdArrival = await _stockArrivalsClient.GetAsync(createdArrivalId, headers);

            Assert.NotNull(createdArrival);
            Assert.Equal(createdArrivalId, createdArrival.Id);
            Assert.Equal(arrival.Type, createdArrival.Type);
            Assert.True(!createdArrival.CreateUserId.IsEmpty());
            Assert.Equal(arrival.IsDeleted, createdArrival.IsDeleted);
            Assert.True(createdArrival.CreateDateTime.IsMoreThanMinValue());
            Assert.NotEmpty(createdArrival.Items);
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var arrival = await _create.StockArrival
                .WithType(StockArrivalType.ArrivalFromSupplier)
                .WithItem(product.Id, 1)
                .BuildAsync();

            var orderId = Guid.NewGuid();

            arrival.Type = StockArrivalType.RefundFromCustomer;
            arrival.OrderId = orderId;
            arrival.IsDeleted = true;
            arrival.Items.First(x => x.ProductId == product.Id).Count = 2;

            await _stockArrivalsClient.UpdateAsync(arrival, headers);

            var updatedArrival = await _stockArrivalsClient.GetAsync(arrival.Id, headers);

            Assert.Equal(arrival.AccountId, updatedArrival.AccountId);
            Assert.Equal(arrival.Type, updatedArrival.Type);
            Assert.Equal(arrival.CreateUserId, updatedArrival.CreateUserId);
            Assert.Equal(arrival.OrderId, updatedArrival.OrderId);
            Assert.Equal(arrival.IsDeleted, updatedArrival.IsDeleted);
            Assert.Equal(arrival.Items.Single().ProductId, updatedArrival.Items.Single().ProductId);
            Assert.Equal(arrival.Items.Single().Count, updatedArrival.Items.Single().Count);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var arrivalIds = (
                    await Task.WhenAll(
                        _create.StockArrival
                            .WithType(StockArrivalType.ArrivalFromSupplier)
                            .WithItem(product.Id, 1)
                            .BuildAsync(),
                        _create.StockArrival
                            .WithType(StockArrivalType.ArrivalFromSupplier)
                            .WithItem(product.Id, 1)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _stockArrivalsClient.DeleteAsync(arrivalIds, headers);

            var arrivals = await _stockArrivalsClient.GetListAsync(arrivalIds, headers);

            Assert.All(arrivals, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var arrivalIds = (
                    await Task.WhenAll(
                        _create.StockArrival
                            .WithType(StockArrivalType.ArrivalFromSupplier)
                            .WithItem(product.Id, 1)
                            .BuildAsync(),
                        _create.StockArrival
                            .WithType(StockArrivalType.ArrivalFromSupplier)
                            .WithItem(product.Id, 1)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _stockArrivalsClient.RestoreAsync(arrivalIds, headers);

            var arrivals = await _stockArrivalsClient.GetListAsync(arrivalIds, headers);

            Assert.All(arrivals, x => Assert.False(x.IsDeleted));
        }
    }
}
