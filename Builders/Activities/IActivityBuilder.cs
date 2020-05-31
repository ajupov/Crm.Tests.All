using System;
using System.Threading.Tasks;
using Crm.V1.Clients.Activities.Models;

namespace Crm.Tests.All.Builders.Activities
{
    public interface IActivityBuilder
    {
        ActivityBuilder WithTypeId(Guid typeId);

        ActivityBuilder WithLeadId(Guid leadId);

        ActivityBuilder WithStatusId(Guid statusId);

        ActivityBuilder WithCompanyId(Guid companyId);

        ActivityBuilder WithContactId(Guid contactId);

        ActivityBuilder WithDealId(Guid dealId);

        ActivityBuilder WithResponsibleUserId(Guid responsibleUserId);

        ActivityBuilder WithName(string name);

        ActivityBuilder WithDescription(string description);

        ActivityBuilder WithResult(string result);

        ActivityBuilder WithPriority(ActivityPriority priority);

        ActivityBuilder WithStartDateTime(DateTime startDateTime);

        ActivityBuilder WithEndDateTime(DateTime endDateTime);

        ActivityBuilder WithDeadLineDateTime(DateTime deadLineDateTime);

        ActivityBuilder AsDeleted();

        ActivityBuilder WithAttributeLink(Guid attributeId, string value);

        Task<Activity> BuildAsync();
    }
}
