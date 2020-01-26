using System;
using System.Threading.Tasks;

namespace Crm.Tests.All.Builders.Leads
{
    public interface ILeadCommentBuilder
    {
        LeadCommentBuilder WithLeadId(Guid leadId);

        Task BuildAsync();
    }
}