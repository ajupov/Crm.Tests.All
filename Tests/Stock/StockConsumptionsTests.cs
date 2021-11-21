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
    public class StockConsumptionsTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IStockConsumptionsClient _stockConsumptionsClient;

        public StockConsumptionsTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IStockConsumptionsClient stockConsumptionsClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _stockConsumptionsClient = stockConsumptionsClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var consumptionId = (
                    await _create.StockConsumption
                        .WithType(StockConsumptionType.SaleToCustomer)
                        .WithOrderId(Guid.NewGuid())
                        .WithItem(product.Id, 1)
                        .BuildAsync())
                .Id;

            var consumption = await _stockConsumptionsClient.GetAsync(consumptionId, headers);

            Assert.NotNull(consumption);
            Assert.Equal(consumptionId, consumption.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var consumptionIds = (
                    await Task.WhenAll(
                        _create.StockConsumption
                            .WithType(StockConsumptionType.SaleToCustomer)
                            .WithOrderId(Guid.NewGuid())
                            .WithItem(product.Id, 1)
                            .BuildAsync(),
                        _create.StockConsumption
                            .WithType(StockConsumptionType.SaleToCustomer)
                            .WithOrderId(Guid.NewGuid())
                            .WithItem(product.Id, 1)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var consumptions = await _stockConsumptionsClient.GetListAsync(consumptionIds, headers);

            Assert.NotEmpty(consumptions);
            Assert.Equal(consumptionIds.Count, consumptions.Count);
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
                _create.StockConsumption
                    .WithType(StockConsumptionType.SaleToCustomer)
                    .WithOrderId(Guid.NewGuid())
                    .WithItem(product.Id, 1)
                    .BuildAsync(),
                _create.StockConsumption
                    .WithType(StockConsumptionType.SaleToCustomer)
                    .WithOrderId(Guid.NewGuid())
                    .WithItem(product.Id, 1)
                    .BuildAsync());

            var request = new StockConsumptionGetPagedListRequest();

            var response = await _stockConsumptionsClient.GetPagedListAsync(request, headers);

            var results = response.Consumptions
                .Skip(1)
                .Zip(response.Consumptions, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Consumptions);
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

            var consumption = new StockConsumption
            {
                Id = Guid.NewGuid(),
                Type = StockConsumptionType.SaleToCustomer,
                OrderId = Guid.NewGuid(),
                IsDeleted = true,
                Items = new List<StockConsumptionItem>
                {
                    new ()
                    {
                        ProductId = product.Id,
                        Count = 1
                    }
                },
            };

            var createdConsumptionId = await _stockConsumptionsClient.CreateAsync(consumption, headers);

            var createdConsumption = await _stockConsumptionsClient.GetAsync(createdConsumptionId, headers);

            Assert.NotNull(createdConsumption);
            Assert.Equal(createdConsumptionId, createdConsumption.Id);
            Assert.Equal(consumption.Type, createdConsumption.Type);
            Assert.True(!createdConsumption.CreateUserId.IsEmpty());
            Assert.Equal(consumption.IsDeleted, createdConsumption.IsDeleted);
            Assert.True(createdConsumption.CreateDateTime.IsMoreThanMinValue());
            Assert.NotEmpty(createdConsumption.Items);
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var consumption = await _create.StockConsumption
                .WithType(StockConsumptionType.ReturnToSupplier)
                .WithItem(product.Id, 1)
                .BuildAsync();

            var orderId = Guid.NewGuid();

            consumption.Type = StockConsumptionType.SaleToCustomer;
            consumption.OrderId = orderId;
            consumption.IsDeleted = true;
            consumption.Items.First(x => x.ProductId == product.Id).Count = 2;

            await _stockConsumptionsClient.UpdateAsync(consumption, headers);

            var updatedConsumption = await _stockConsumptionsClient.GetAsync(consumption.Id, headers);

            Assert.Equal(consumption.AccountId, updatedConsumption.AccountId);
            Assert.Equal(consumption.Type, updatedConsumption.Type);
            Assert.Equal(consumption.CreateUserId, updatedConsumption.CreateUserId);
            Assert.Equal(consumption.OrderId, updatedConsumption.OrderId);
            Assert.Equal(consumption.IsDeleted, updatedConsumption.IsDeleted);
            Assert.Equal(consumption.Items.Single().ProductId, updatedConsumption.Items.Single().ProductId);
            Assert.Equal(consumption.Items.Single().Count, updatedConsumption.Items.Single().Count);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var consumptionIds = (
                    await Task.WhenAll(
                        _create.StockConsumption
                            .WithType(StockConsumptionType.SaleToCustomer)
                            .WithOrderId(Guid.NewGuid())
                            .WithItem(product.Id, 1)
                            .BuildAsync(),
                        _create.StockConsumption
                            .WithType(StockConsumptionType.SaleToCustomer)
                            .WithOrderId(Guid.NewGuid())
                            .WithItem(product.Id, 1)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _stockConsumptionsClient.DeleteAsync(consumptionIds, headers);

            var consumptions = await _stockConsumptionsClient.GetListAsync(consumptionIds, headers);

            Assert.All(consumptions, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var consumptionIds = (
                    await Task.WhenAll(
                        _create.StockConsumption
                            .WithType(StockConsumptionType.SaleToCustomer)
                            .WithOrderId(Guid.NewGuid())
                            .WithItem(product.Id, 1)
                            .BuildAsync(),
                        _create.StockConsumption
                            .WithType(StockConsumptionType.SaleToCustomer)
                            .WithOrderId(Guid.NewGuid())
                            .WithItem(product.Id, 1)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _stockConsumptionsClient.RestoreAsync(consumptionIds, headers);

            var consumptions = await _stockConsumptionsClient.GetListAsync(consumptionIds, headers);

            Assert.All(consumptions, x => Assert.False(x.IsDeleted));
        }
    }
}
