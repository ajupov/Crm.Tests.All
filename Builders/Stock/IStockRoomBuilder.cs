using System.Threading.Tasks;
using Crm.v1.Clients.Stock.Models;

namespace Crm.Tests.All.Builders.Stock
{
    public interface IStockRoomBuilder
    {
        StockRoomBuilder WithName(string name);

        StockRoomBuilder AsDeleted();

        Task<StockRoom> BuildAsync();
    }
}
