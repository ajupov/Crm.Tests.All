using Ajupov.Infrastructure.All.Configuration;
using Crm.Tests.All.Builders.Customers;
using Crm.Tests.All.Builders.OAuth;
using Crm.Tests.All.Builders.Orders;
using Crm.Tests.All.Builders.Products;
using Crm.Tests.All.Builders.Tasks;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.Tests.All.Settings;
using Crm.v1.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Tests.All
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = Configuration.GetConfiguration();

            var hostsSettings = configuration.GetSection(nameof(HostsSettings));
            var oauthSettings = configuration.GetSection(nameof(OAuthSettings));
            var clientId = oauthSettings.GetValue<string>(nameof(OAuthSettings.ClientId));
            var apiHost = hostsSettings.GetValue<string>(nameof(HostsSettings.ApiHost));
            var oauthHost = hostsSettings.GetValue<string>(nameof(HostsSettings.OAuthHost));

            services
                .ConfigureClients(clientId, apiHost, oauthHost);

            services
                .AddSingleton<IDefaultRequestHeadersService, DefaultRequestHeadersService>()
                .AddTransient<ICreate, Create>();

            services
                .Configure<OAuthSettings>(configuration.GetSection(nameof(OAuthSettings)));

            services
                .AddTransient<IOAuthBuilder, OAuthBuilder>()
                .AddTransient<IProductBuilder, ProductBuilder>()
                .AddTransient<IProductAttributeBuilder, ProductAttributeBuilder>()
                .AddTransient<IProductCategoryBuilder, ProductCategoryBuilder>()
                .AddTransient<IProductStatusBuilder, ProductStatusBuilder>()
                .AddTransient<ICustomerBuilder, CustomerBuilder>()
                .AddTransient<ICustomerAttributeBuilder, CustomerAttributeBuilder>()
                .AddTransient<ICustomerCommentBuilder, CustomerCommentBuilder>()
                .AddTransient<ICustomerSourceBuilder, CustomerSourceBuilder>()
                .AddTransient<IOrderBuilder, OrderBuilder>()
                .AddTransient<IOrderAttributeBuilder, OrderAttributeBuilder>()
                .AddTransient<IOrderCommentBuilder, OrderCommentBuilder>()
                .AddTransient<IOrderStatusBuilder, OrderStatusBuilder>()
                .AddTransient<IOrderTypeBuilder, OrderTypeBuilder>()
                .AddTransient<ITaskBuilder, TaskBuilder>()
                .AddTransient<ITaskAttributeBuilder, TaskAttributeBuilder>()
                .AddTransient<ITaskCommentBuilder, TaskCommentBuilder>()
                .AddTransient<ITaskStatusBuilder, TaskStatusBuilder>()
                .AddTransient<ITaskTypeBuilder, TaskTypeBuilder>();
        }
    }
}
