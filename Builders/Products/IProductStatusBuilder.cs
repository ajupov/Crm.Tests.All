using System.Threading.Tasks;
using Crm.v1.Clients.Products.Models;

namespace Crm.Tests.All.Builders.Products
{
    public interface IProductStatusBuilder
    {
        ProductStatusBuilder WithName(string name);

        ProductStatusBuilder IsDeleted();

        Task<ProductStatus> BuildAsync();
    }
}