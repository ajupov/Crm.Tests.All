using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Stock.Clients;
using Crm.v1.Clients.Stock.Models;

namespace Crm.Tests.All.Builders.Stock
{
    public class StockConsumptionBuilder : IStockConsumptionBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IStockConsumptionsClient _consumptionsClient;
        private readonly StockConsumption _consumption;

        public StockConsumptionBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IStockConsumptionsClient consumptionsClient)
        {
            _consumptionsClient = consumptionsClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _consumption = new StockConsumption
            {
                Id = Guid.NewGuid(),
                Type = StockConsumptionType.SaleToCustomer,
                IsDeleted = false
            };
        }

        public StockConsumptionBuilder WithCreateUserId(Guid createUserId)
        {
            _consumption.CreateUserId = createUserId;

            return this;
        }

        public StockConsumptionBuilder WithType(StockConsumptionType type)
        {
            _consumption.Type = type;

            return this;
        }

        public StockConsumptionBuilder WithOrderId(Guid orderId)
        {
            _consumption.OrderId = orderId;

            return this;
        }

        public StockConsumptionBuilder AsDeleted()
        {
            _consumption.IsDeleted = true;

            return this;
        }

        public StockConsumptionBuilder WithItem(Guid productId, decimal count)
        {
            _consumption.Items ??= new List<StockConsumptionItem>();
            _consumption.Items.Add(new StockConsumptionItem
            {
                ProductId = productId,
                Count = count
            });

            return this;
        }

        public async Task<StockConsumption> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var id = await _consumptionsClient.CreateAsync(_consumption, headers);

            return await _consumptionsClient.GetAsync(id, headers);
        }
    }
}
