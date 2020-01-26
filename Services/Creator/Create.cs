using System;
using Crm.Tests.All.Builders.Activities;
using Crm.Tests.All.Builders.Companies;
using Crm.Tests.All.Builders.Contacts;
using Crm.Tests.All.Builders.Deals;
using Crm.Tests.All.Builders.Leads;
using Crm.Tests.All.Builders.OAuth;
using Crm.Tests.All.Builders.Products;
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

        public ILeadBuilder Lead => _services.GetService<ILeadBuilder>();

        public ILeadSourceBuilder LeadSource => _services.GetService<ILeadSourceBuilder>();

        public ILeadAttributeBuilder LeadAttribute => _services.GetService<ILeadAttributeBuilder>();

        public ILeadCommentBuilder LeadComment => _services.GetService<ILeadCommentBuilder>();

        public ICompanyBuilder Company => _services.GetService<ICompanyBuilder>();

        public ICompanyAttributeBuilder CompanyAttribute => _services.GetService<ICompanyAttributeBuilder>();

        public ICompanyCommentBuilder CompanyComment => _services.GetService<ICompanyCommentBuilder>();

        public IContactBuilder Contact => _services.GetService<IContactBuilder>();

        public IContactAttributeBuilder ContactAttribute => _services.GetService<IContactAttributeBuilder>();

        public IContactCommentBuilder ContactComment => _services.GetService<IContactCommentBuilder>();

        public IDealBuilder Deal => _services.GetService<IDealBuilder>();

        public IDealStatusBuilder DealStatus => _services.GetService<IDealStatusBuilder>();

        public IDealTypeBuilder DealType => _services.GetService<IDealTypeBuilder>();

        public IDealAttributeBuilder DealAttribute => _services.GetService<IDealAttributeBuilder>();

        public IDealCommentBuilder DealComment => _services.GetService<IDealCommentBuilder>();

        public IActivityBuilder Activity => _services.GetService<IActivityBuilder>();

        public IActivityStatusBuilder ActivityStatus => _services.GetService<IActivityStatusBuilder>();

        public IActivityTypeBuilder ActivityType => _services.GetService<IActivityTypeBuilder>();

        public IActivityAttributeBuilder ActivityAttribute => _services.GetService<IActivityAttributeBuilder>();

        public IActivityCommentBuilder ActivityComment => _services.GetService<IActivityCommentBuilder>();
    }
}