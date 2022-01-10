using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Stock.Clients;
using Crm.v1.Clients.Stock.Models;

namespace Crm.Tests.All.Builders.Stock
{
    public class StockBalanceBuilder : IStockBalanceBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IStockBalancesClient _balancesClient;
        private readonly StockBalance _balance;

        public StockBalanceBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IStockBalancesClient balancesClient)
        {
            _balancesClient = balancesClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _balance = new StockBalance
            {
                Id = Guid.NewGuid(),
                Count = 0,
                IsDeleted = false
            };
        }

        public StockBalanceBuilder WithCreateUserId(Guid createUserId)
        {
            _balance.CreateUserId = createUserId;

            return this;
        }

        public StockBalanceBuilder WithRoomId(Guid roomId)
        {
            _balance.RoomId = roomId;

            return this;
        }

        public StockBalanceBuilder WithProductId(Guid productId)
        {
            _balance.ProductId = productId;

            return this;
        }

        public StockBalanceBuilder WithCount(decimal count)
        {
            _balance.Count = count;

            return this;
        }

        public StockBalanceBuilder AsDeleted()
        {
            _balance.IsDeleted = true;

            return this;
        }

        public async Task<StockBalance> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            if (_balance.RoomId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_balance.RoomId));
            }

            if (_balance.ProductId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_balance.ProductId));
            }

            var id = await _balancesClient.CreateAsync(_balance, headers);

            return await _balancesClient.GetAsync(id, headers);
        }
    }
}
