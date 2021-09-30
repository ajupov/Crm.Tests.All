using System;
using System.Threading.Tasks;
using Crm.v1.Clients.Products.Models;

namespace Crm.Tests.All.Builders.Products
{
    public interface IProductBuilder
    {
        ProductBuilder WithParentProductId(Guid productId);

        ProductBuilder WithType(ProductType type);

        ProductBuilder WithStatusId(Guid statusId);

        ProductBuilder WithName(string name);

        ProductBuilder WithVendorCode(string vendorCode);

        ProductBuilder WithPrice(decimal price);

        ProductBuilder AsHidden();

        ProductBuilder AsDeleted();

        ProductBuilder WithAttributeLink(Guid attributeId, string value);

        ProductBuilder WithCategoryLink(Guid categoryId);

        Task<Product> BuildAsync();
    }
}
