using System.Threading.Tasks;

namespace Crm.Tests.All.Services.AccessTokenGetter
{
    public interface IAccessTokenGetter
    {
        Task<string> GetAsync();
    }
}