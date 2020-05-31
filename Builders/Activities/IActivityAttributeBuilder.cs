using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.V1.Clients.Activities.Models;

namespace Crm.Tests.All.Builders.Activities
{
    public interface IActivityAttributeBuilder
    {
        ActivityAttributeBuilder WithType(AttributeType type);

        ActivityAttributeBuilder WithKey(string key);

        ActivityAttributeBuilder AsDeleted();

        Task<ActivityAttribute> BuildAsync();
    }
}
