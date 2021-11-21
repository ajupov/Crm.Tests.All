using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Stock.Clients;
using Crm.v1.Clients.Stock.Models;

namespace Crm.Tests.All.Builders.Stock
{
    public class StockArrivalBuilder : IStockArrivalBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IStockArrivalsClient _arrivalsClient;
        private readonly StockArrival _arrival;

        public StockArrivalBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IStockArrivalsClient arrivalsClient)
        {
            _arrivalsClient = arrivalsClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _arrival = new StockArrival
            {
                Id = Guid.NewGuid(),
                Type = StockArrivalType.ArrivalFromSupplier,
                IsDeleted = false
            };
        }

        public StockArrivalBuilder WithCreateUserId(Guid createUserId)
        {
            _arrival.CreateUserId = createUserId;

            return this;
        }

        public StockArrivalBuilder WithType(StockArrivalType type)
        {
            _arrival.Type = type;

            return this;
        }

        public StockArrivalBuilder WithOrderId(Guid orderId)
        {
            _arrival.OrderId = orderId;

            return this;
        }

        public StockArrivalBuilder AsDeleted()
        {
            _arrival.IsDeleted = true;

            return this;
        }

        public StockArrivalBuilder WithItem(Guid productId, decimal count)
        {
            _arrival.Items ??= new List<StockArrivalItem>();
            _arrival.Items.Add(new StockArrivalItem
            {
                ProductId = productId,
                Count = count
            });

            return this;
        }

        public async Task<StockArrival> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            if (_arrival.Items == null || !_arrival.Items.Any())
            {
                throw new InvalidOperationException(nameof(_arrival.Items));
            }

            var id = await _arrivalsClient.CreateAsync(_arrival, headers);

            return await _arrivalsClient.GetAsync(id, headers);
        }
    }
}
