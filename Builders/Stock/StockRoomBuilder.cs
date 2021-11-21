using System;
using System.Threading.Tasks;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Stock.Clients;
using Crm.v1.Clients.Stock.Models;

namespace Crm.Tests.All.Builders.Stock
{
    public class StockRoomBuilder : IStockRoomBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IStockRoomsClient _orderTypesClient;
        private readonly StockRoom _room;

        public StockRoomBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IStockRoomsClient orderTypesClient)
        {
            _orderTypesClient = orderTypesClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _room = new StockRoom
            {
                Id = Guid.NewGuid(),
                Name = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public StockRoomBuilder WithName(string name)
        {
            _room.Name = name;

            return this;
        }

        public StockRoomBuilder AsDeleted()
        {
            _room.IsDeleted = true;

            return this;
        }

        public async Task<StockRoom> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var id = await _orderTypesClient.CreateAsync(_room, headers);

            return await _orderTypesClient.GetAsync(id, headers);
        }
    }
}
