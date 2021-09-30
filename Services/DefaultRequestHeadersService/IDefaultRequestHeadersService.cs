using System.Collections.Generic;
using System.Threading.Tasks;

namespace Crm.Tests.All.Services.DefaultRequestHeadersService
{
    public interface IDefaultRequestHeadersService
    {
        Task<Dictionary<string, string>> GetAsync();
    }
}
