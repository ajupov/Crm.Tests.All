using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.v1.Clients.Suppliers.Models;

namespace Crm.Tests.All.Builders.Suppliers
{
    public interface ISupplierAttributeBuilder
    {
        SupplierAttributeBuilder WithType(AttributeType type);

        SupplierAttributeBuilder WithKey(string key);

        SupplierAttributeBuilder AsDeleted();

        Task<SupplierAttribute> BuildAsync();
    }
}
