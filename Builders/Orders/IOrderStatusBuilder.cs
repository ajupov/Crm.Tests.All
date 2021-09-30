using System.Threading.Tasks;
using Crm.v1.Clients.Orders.Models;

namespace Crm.Tests.All.Builders.Orders
{
    public interface IOrderStatusBuilder
    {
        OrderStatusBuilder WithName(string name);

        OrderStatusBuilder AsFinish();

        OrderStatusBuilder AsDeleted();

        Task<OrderStatus> BuildAsync();
    }
}
