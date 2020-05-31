using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.V1.Clients.Leads.Models;

namespace Crm.Tests.All.Builders.Leads
{
    public interface ILeadAttributeBuilder
    {
        LeadAttributeBuilder WithType(AttributeType type);

        LeadAttributeBuilder WithKey(string key);

        LeadAttributeBuilder AsDeleted();

        Task<LeadAttribute> BuildAsync();
    }
}
