using System;
using System.Threading.Tasks;

namespace Crm.Tests.All.Builders.Deals
{
    public interface IDealCommentBuilder
    {
        DealCommentBuilder WithDealId(Guid dealId);

        Task BuildAsync();
    }
}