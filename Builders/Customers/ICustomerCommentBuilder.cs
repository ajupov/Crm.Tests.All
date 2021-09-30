using System;
using System.Threading.Tasks;

namespace Crm.Tests.All.Builders.Customers
{
    public interface ICustomerCommentBuilder
    {
        CustomerCommentBuilder WithCustomerId(Guid customerId);

        Task BuildAsync();
    }
}
