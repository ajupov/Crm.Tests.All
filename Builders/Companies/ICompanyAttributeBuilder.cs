using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.v1.Clients.Companies.Models;

namespace Crm.Tests.All.Builders.Companies
{
    public interface ICompanyAttributeBuilder
    {
        CompanyAttributeBuilder WithType(AttributeType type);

        CompanyAttributeBuilder WithKey(string key);

        CompanyAttributeBuilder AsDeleted();

        Task<CompanyAttribute> BuildAsync();
    }
}