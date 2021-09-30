using System;
using System.Threading.Tasks;

namespace Crm.Tests.All.Builders.Orders
{
    public interface IOrderCommentBuilder
    {
        OrderCommentBuilder WithOrderId(Guid orderId);

        Task BuildAsync();
    }
}
