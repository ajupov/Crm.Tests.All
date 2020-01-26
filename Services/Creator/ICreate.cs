using Crm.Tests.All.Builders.Activities;
using Crm.Tests.All.Builders.Companies;
using Crm.Tests.All.Builders.Contacts;
using Crm.Tests.All.Builders.Deals;
using Crm.Tests.All.Builders.Leads;
using Crm.Tests.All.Builders.OAuth;
using Crm.Tests.All.Builders.Products;

namespace Crm.Tests.All.Services.Creator
{
    public interface ICreate
    {
        IOAuthBuilder OAuth { get; }

        IProductBuilder Product { get; }

        IProductCategoryBuilder ProductCategory { get; }

        IProductStatusBuilder ProductStatus { get; }

        IProductAttributeBuilder ProductAttribute { get; }

        ILeadBuilder Lead { get; }

        ILeadSourceBuilder LeadSource { get; }

        ILeadAttributeBuilder LeadAttribute { get; }

        ILeadCommentBuilder LeadComment { get; }

        ICompanyBuilder Company { get; }

        ICompanyAttributeBuilder CompanyAttribute { get; }

        ICompanyCommentBuilder CompanyComment { get; }

        IContactBuilder Contact { get; }

        IContactAttributeBuilder ContactAttribute { get; }

        IContactCommentBuilder ContactComment { get; }

        IDealBuilder Deal { get; }

        IDealStatusBuilder DealStatus { get; }

        IDealTypeBuilder DealType { get; }

        IDealAttributeBuilder DealAttribute { get; }

        IDealCommentBuilder DealComment { get; }

        IActivityBuilder Activity { get; }

        IActivityStatusBuilder ActivityStatus { get; }

        IActivityTypeBuilder ActivityType { get; }

        IActivityAttributeBuilder ActivityAttribute { get; }

        IActivityCommentBuilder ActivityComment { get; }
    }
}