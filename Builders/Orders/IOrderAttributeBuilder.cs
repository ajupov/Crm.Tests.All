using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.v1.Clients.Orders.Models;

namespace Crm.Tests.All.Builders.Orders
{
    public interface IOrderAttributeBuilder
    {
        OrderAttributeBuilder WithType(AttributeType type);

        OrderAttributeBuilder WithKey(string key);

        OrderAttributeBuilder AsDeleted();

        Task<OrderAttribute> BuildAsync();
    }
}
