using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.v1.Clients.Customers.Models;

namespace Crm.Tests.All.Builders.Customers
{
    public interface ICustomerAttributeBuilder
    {
        CustomerAttributeBuilder WithType(AttributeType type);

        CustomerAttributeBuilder WithKey(string key);

        CustomerAttributeBuilder AsDeleted();

        Task<CustomerAttribute> BuildAsync();
    }
}
