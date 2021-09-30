using System;
using System.Threading.Tasks;

namespace Crm.Tests.All.Builders.Tasks
{
    public interface ITaskCommentBuilder
    {
        TaskCommentBuilder WithTaskId(Guid taskId);

        Task BuildAsync();
    }
}
