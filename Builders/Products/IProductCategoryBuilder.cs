using System.Threading.Tasks;
using Crm.V1.Clients.Products.Models;

namespace Crm.Tests.All.Builders.Products
{
    public interface IProductCategoryBuilder
    {
        ProductCategoryBuilder WithName(string name);

        ProductCategoryBuilder IsDeleted();

        Task<ProductCategory> BuildAsync();
    }
}
