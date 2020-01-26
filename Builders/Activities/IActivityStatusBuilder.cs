using System.Threading.Tasks;
using Crm.v1.Clients.Activities.Models;

namespace Crm.Tests.All.Builders.Activities
{
    public interface IActivityStatusBuilder
    {
        ActivityStatusBuilder WithName(string name);

        ActivityStatusBuilder AsFinish();

        ActivityStatusBuilder AsDeleted();

        Task<ActivityStatus> BuildAsync();
    }
}