using System;
using System.Threading.Tasks;
using Crm.v1.Clients.Stock.Models;

namespace Crm.Tests.All.Builders.Stock
{
    public interface IStockBalanceBuilder
    {
        StockBalanceBuilder WithCreateUserId(Guid createUserId);

        StockBalanceBuilder WithRoomId(Guid roomId);

        StockBalanceBuilder WithProductId(Guid productId);

        StockBalanceBuilder WithCount(decimal count);

        StockBalanceBuilder AsDeleted();

        StockBalanceBuilder WithUniqueElementId(Guid uniqueElementId);

        Task<StockBalance> BuildAsync();
    }
}
