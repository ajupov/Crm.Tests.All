using System;
using System.Threading.Tasks;
using Crm.v1.Clients.Tasks.Models;
using CrmTask = Crm.v1.Clients.Tasks.Models.Task;

namespace Crm.Tests.All.Builders.Tasks
{
    public interface ITaskBuilder
    {
        TaskBuilder WithTypeId(Guid typeId);

        TaskBuilder WithCustomerId(Guid customerId);

        TaskBuilder WithStatusId(Guid statusId);

        TaskBuilder WithOrderId(Guid orderId);

        TaskBuilder WithResponsibleUserId(Guid responsibleUserId);

        TaskBuilder WithName(string name);

        TaskBuilder WithDescription(string description);

        TaskBuilder WithResult(string result);

        TaskBuilder WithPriority(TaskPriority priority);

        TaskBuilder WithStartDateTime(DateTime startDateTime);

        TaskBuilder WithEndDateTime(DateTime endDateTime);

        TaskBuilder WithDeadLineDateTime(DateTime deadLineDateTime);

        TaskBuilder AsDeleted();

        TaskBuilder WithAttributeLink(Guid attributeId, string value);

        Task<CrmTask> BuildAsync();
    }
}
