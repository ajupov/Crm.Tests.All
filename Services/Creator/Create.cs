using System;
using Crm.Tests.All.Builders.Customers;
using Crm.Tests.All.Builders.OAuth;
using Crm.Tests.All.Builders.Orders;
using Crm.Tests.All.Builders.Products;
using Crm.Tests.All.Builders.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Tests.All.Services.Creator
{
    public class Create : ICreate
    {
        private readonly IServiceProvider _services;

        public Create(IServiceProvider services)
        {
            _services = services;
        }

        public IOAuthBuilder OAuth => _services.GetService<IOAuthBuilder>();

        public IProductBuilder Product => _services.GetService<IProductBuilder>();

        public IProductCategoryBuilder ProductCategory => _services.GetService<IProductCategoryBuilder>();

        public IProductStatusBuilder ProductStatus => _services.GetService<IProductStatusBuilder>();

        public IProductAttributeBuilder ProductAttribute => _services.GetService<IProductAttributeBuilder>();

        public ICustomerBuilder Customer => _services.GetService<ICustomerBuilder>();

        public ICustomerSourceBuilder CustomerSource => _services.GetService<ICustomerSourceBuilder>();

        public ICustomerAttributeBuilder CustomerAttribute => _services.GetService<ICustomerAttributeBuilder>();

        public ICustomerCommentBuilder CustomerComment => _services.GetService<ICustomerCommentBuilder>();

        public IOrderBuilder Order => _services.GetService<IOrderBuilder>();

        public IOrderStatusBuilder OrderStatus => _services.GetService<IOrderStatusBuilder>();

        public IOrderTypeBuilder OrderType => _services.GetService<IOrderTypeBuilder>();

        public IOrderAttributeBuilder OrderAttribute => _services.GetService<IOrderAttributeBuilder>();

        public IOrderCommentBuilder OrderComment => _services.GetService<IOrderCommentBuilder>();

        public ITaskBuilder Task => _services.GetService<ITaskBuilder>();

        public ITaskStatusBuilder TaskStatus => _services.GetService<ITaskStatusBuilder>();

        public ITaskTypeBuilder TaskType => _services.GetService<ITaskTypeBuilder>();

        public ITaskAttributeBuilder TaskAttribute => _services.GetService<ITaskAttributeBuilder>();

        public ITaskCommentBuilder TaskComment => _services.GetService<ITaskCommentBuilder>();
    }
}
