using System.Threading.Tasks;
using Crm.V1.Clients.Leads.Models;

namespace Crm.Tests.All.Builders.Leads
{
    public interface ILeadSourceBuilder
    {
        LeadSourceBuilder WithName(string name);

        LeadSourceBuilder AsDeleted();

        Task<LeadSource> BuildAsync();
    }
}
