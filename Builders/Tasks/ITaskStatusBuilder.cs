using System.Threading.Tasks;
using CrmTaskStatus = Crm.v1.Clients.Tasks.Models.TaskStatus;

namespace Crm.Tests.All.Builders.Tasks
{
    public interface ITaskStatusBuilder
    {
        TaskStatusBuilder WithName(string name);

        TaskStatusBuilder AsFinish();

        TaskStatusBuilder AsDeleted();

        Task<CrmTaskStatus> BuildAsync();
    }
}
