using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.v1.Clients.Deals.Models;

namespace Crm.Tests.All.Builders.Deals
{
    public interface IDealAttributeBuilder
    {
        DealAttributeBuilder WithType(AttributeType type);

        DealAttributeBuilder WithKey(string key);

        DealAttributeBuilder AsDeleted();

        Task<DealAttribute> BuildAsync();
    }
}