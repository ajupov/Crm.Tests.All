using System.Threading.Tasks;
using Crm.V1.Clients.Deals.Models;

namespace Crm.Tests.All.Builders.Deals
{
    public interface IDealTypeBuilder
    {
        DealTypeBuilder WithName(string name);

        DealTypeBuilder AsDeleted();

        Task<DealType> BuildAsync();
    }
}
