using System;
using System.Threading.Tasks;
using Crm.v1.Clients.Stock.Models;

namespace Crm.Tests.All.Builders.Stock
{
    public interface IStockArrivalBuilder
    {
        StockArrivalBuilder WithCreateUserId(Guid createUserId);

        StockArrivalBuilder WithType(StockArrivalType type);

        StockArrivalBuilder WithOrderId(Guid orderId);

        StockArrivalBuilder AsDeleted();

        StockArrivalBuilder WithItem(Guid roomId, Guid productId, decimal count);

        Task<StockArrival> BuildAsync();
    }
}
