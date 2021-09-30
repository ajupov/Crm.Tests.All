using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.v1.Clients.Tasks.Models;

namespace Crm.Tests.All.Builders.Tasks
{
    public interface ITaskAttributeBuilder
    {
        TaskAttributeBuilder WithType(AttributeType type);

        TaskAttributeBuilder WithKey(string key);

        TaskAttributeBuilder AsDeleted();

        Task<TaskAttribute> BuildAsync();
    }
}
