using Crm.Tests.All.Builders.Customers;
using Crm.Tests.All.Builders.OAuth;
using Crm.Tests.All.Builders.Orders;
using Crm.Tests.All.Builders.Products;
using Crm.Tests.All.Builders.Suppliers;
using Crm.Tests.All.Builders.Tasks;

namespace Crm.Tests.All.Services.Creator
{
    public interface ICreate
    {
        IOAuthBuilder OAuth { get; }

        IProductBuilder Product { get; }

        IProductCategoryBuilder ProductCategory { get; }

        IProductStatusBuilder ProductStatus { get; }

        IProductAttributeBuilder ProductAttribute { get; }

        ICustomerBuilder Customer { get; }

        ICustomerSourceBuilder CustomerSource { get; }

        ICustomerAttributeBuilder CustomerAttribute { get; }

        ICustomerCommentBuilder CustomerComment { get; }

        IOrderBuilder Order { get; }

        IOrderStatusBuilder OrderStatus { get; }

        IOrderTypeBuilder OrderType { get; }

        IOrderAttributeBuilder OrderAttribute { get; }

        IOrderCommentBuilder OrderComment { get; }

        ITaskBuilder Task { get; }

        ITaskStatusBuilder TaskStatus { get; }

        ITaskTypeBuilder TaskType { get; }

        ITaskAttributeBuilder TaskAttribute { get; }

        ITaskCommentBuilder TaskComment { get; }

        ISupplierBuilder Supplier { get; }

        ISupplierAttributeBuilder SupplierAttribute { get; }

        ISupplierCommentBuilder SupplierComment { get; }
    }
}
