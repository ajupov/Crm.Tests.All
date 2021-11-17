using System;
using System.Threading.Tasks;

namespace Crm.Tests.All.Builders.Suppliers
{
    public interface ISupplierCommentBuilder
    {
        SupplierCommentBuilder WithSupplierId(Guid customerId);

        Task BuildAsync();
    }
}
