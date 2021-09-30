using System.Threading.Tasks;
using Crm.v1.Clients.Orders.Models;

namespace Crm.Tests.All.Builders.Orders
{
    public interface IOrderTypeBuilder
    {
        OrderTypeBuilder WithName(string name);

        OrderTypeBuilder AsDeleted();

        Task<OrderType> BuildAsync();
    }
}
