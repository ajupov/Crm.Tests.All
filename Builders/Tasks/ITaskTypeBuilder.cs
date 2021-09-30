using System.Threading.Tasks;
using Crm.v1.Clients.Tasks.Models;

namespace Crm.Tests.All.Builders.Tasks
{
    public interface ITaskTypeBuilder
    {
        TaskTypeBuilder WithName(string name);

        TaskTypeBuilder AsDeleted();

        Task<TaskType> BuildAsync();
    }
}
