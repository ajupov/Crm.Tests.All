using System.Threading.Tasks;
using Crm.V1.Clients.OAuth.Models;

namespace Crm.Tests.All.Builders.OAuth
{
    public interface IOAuthBuilder
    {
        Task<Tokens> BuildAsync();
    }
}
