using Ajupov.Infrastructure.All.Configuration;
using Ajupov.Infrastructure.All.TestsDependencyInjection;
using Ajupov.Infrastructure.All.TestsDependencyInjection.Attributes;
using Crm.Tests.All.Builders.Activities;
using Crm.Tests.All.Builders.Companies;
using Crm.Tests.All.Builders.Contacts;
using Crm.Tests.All.Builders.Deals;
using Crm.Tests.All.Builders.Leads;
using Crm.Tests.All.Builders.OAuth;
using Crm.Tests.All.Builders.Products;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Settings;
using Crm.v1.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: DependencyInject("Crm.Tests.All.Startup", "Crm.Tests.All")]

namespace Crm.Tests.All
{
    public class Startup : BaseStartup
    {
        protected override void Configure(IServiceCollection services)
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
                .AddSingleton<IAccessTokenGetter, AccessTokenGetter>()
                .AddTransient<ICreate, Create>();

            services
                .Configure<OAuthSettings>(configuration.GetSection(nameof(OAuthSettings)));

            services
                .AddTransient<IOAuthBuilder, OAuthBuilder>()
                .AddTransient<IProductBuilder, ProductBuilder>()
                .AddTransient<IProductAttributeBuilder, ProductAttributeBuilder>()
                .AddTransient<IProductCategoryBuilder, ProductCategoryBuilder>()
                .AddTransient<IProductStatusBuilder, ProductStatusBuilder>()
                .AddTransient<ILeadBuilder, LeadBuilder>()
                .AddTransient<ILeadAttributeBuilder, LeadAttributeBuilder>()
                .AddTransient<ILeadCommentBuilder, LeadCommentBuilder>()
                .AddTransient<ILeadSourceBuilder, LeadSourceBuilder>()
                .AddTransient<ICompanyBuilder, CompanyBuilder>()
                .AddTransient<ICompanyAttributeBuilder, CompanyAttributeBuilder>()
                .AddTransient<ICompanyCommentBuilder, CompanyCommentBuilder>()
                .AddTransient<IContactBuilder, ContactBuilder>()
                .AddTransient<IContactAttributeBuilder, ContactAttributeBuilder>()
                .AddTransient<IContactCommentBuilder, ContactCommentBuilder>()
                .AddTransient<IDealBuilder, DealBuilder>()
                .AddTransient<IDealAttributeBuilder, DealAttributeBuilder>()
                .AddTransient<IDealCommentBuilder, DealCommentBuilder>()
                .AddTransient<IDealStatusBuilder, DealStatusBuilder>()
                .AddTransient<IDealTypeBuilder, DealTypeBuilder>()
                .AddTransient<IActivityBuilder, ActivityBuilder>()
                .AddTransient<IActivityAttributeBuilder, ActivityAttributeBuilder>()
                .AddTransient<IActivityCommentBuilder, ActivityCommentBuilder>()
                .AddTransient<IActivityStatusBuilder, ActivityStatusBuilder>()
                .AddTransient<IActivityTypeBuilder, ActivityTypeBuilder>();
        }
    }
}