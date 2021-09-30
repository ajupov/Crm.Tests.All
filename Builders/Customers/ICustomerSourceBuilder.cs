using System.Threading.Tasks;
using Crm.v1.Clients.Customers.Models;

namespace Crm.Tests.All.Builders.Customers
{
    public interface ICustomerSourceBuilder
    {
        CustomerSourceBuilder WithName(string name);

        CustomerSourceBuilder AsDeleted();

        Task<CustomerSource> BuildAsync();
    }
}
