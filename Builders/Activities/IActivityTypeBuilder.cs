using System.Threading.Tasks;
using Crm.v1.Clients.Activities.Models;

namespace Crm.Tests.All.Builders.Activities
{
    public interface IActivityTypeBuilder
    {
        ActivityTypeBuilder WithName(string name);

        ActivityTypeBuilder AsDeleted();

        Task<ActivityType> BuildAsync();
    }
}