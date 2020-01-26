using System.Threading.Tasks;
using Crm.v1.Clients.Deals.Models;

namespace Crm.Tests.All.Builders.Deals
{
    public interface IDealStatusBuilder
    {
        DealStatusBuilder WithName(string name);

        DealStatusBuilder AsFinish();

        DealStatusBuilder AsDeleted();

        Task<DealStatus> BuildAsync();
    }
}