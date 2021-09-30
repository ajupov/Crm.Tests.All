using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.v1.Clients.Products.Models;

namespace Crm.Tests.All.Builders.Products
{
    public interface IProductAttributeBuilder
    {
        ProductAttributeBuilder WithType(AttributeType type);

        ProductAttributeBuilder WithKey(string key);

        ProductAttributeBuilder AsDeleted();

        Task<ProductAttribute> BuildAsync();
    }
}
