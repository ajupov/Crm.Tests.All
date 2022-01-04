using System;
using System.Threading.Tasks;
using Crm.v1.Clients.Stock.Models;

namespace Crm.Tests.All.Builders.Stock
{
    public interface IStockConsumptionBuilder
    {
        StockConsumptionBuilder WithCreateUserId(Guid createUserId);

        StockConsumptionBuilder WithType(StockConsumptionType type);

        StockConsumptionBuilder WithOrderId(Guid orderId);

        StockConsumptionBuilder AsDeleted();

        StockConsumptionBuilder WithItem(Guid roomId, Guid productId, decimal count);

        Task<StockConsumption> BuildAsync();
    }
}
