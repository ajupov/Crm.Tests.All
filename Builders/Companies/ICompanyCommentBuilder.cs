using System;
using System.Threading.Tasks;

namespace Crm.Tests.All.Builders.Companies
{
    public interface ICompanyCommentBuilder
    {
        CompanyCommentBuilder WithCompanyId(Guid companyId);

        Task BuildAsync();
    }
}