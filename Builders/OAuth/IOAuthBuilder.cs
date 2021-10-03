using System.Threading.Tasks;
using Crm.v1.Clients.OAuth.Models;

namespace Crm.Tests.All.Builders.OAuth
{
    public interface IOAuthBuilder
    {
        Task<TokenResponse> BuildAsync();
    }
}
