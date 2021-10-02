using System;
using System.Threading.Tasks;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Orders.Clients;
using Crm.v1.Clients.Orders.Models;

namespace Crm.Tests.All.Builders.Orders
{
    public class OrderCommentBuilder : IOrderCommentBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IOrderCommentsClient _orderCommentsClient;
        private readonly OrderComment _comment;

        public OrderCommentBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IOrderCommentsClient orderCommentsClient)
        {
            _orderCommentsClient = orderCommentsClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _comment = new OrderComment
            {
                Id = Guid.NewGuid(),
                Value = "Test".WithGuid()
            };
        }

        public OrderCommentBuilder WithOrderId(Guid orderId)
        {
            _comment.OrderId = orderId;

            return this;
        }

        public async Task BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            if (_comment.OrderId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_comment.OrderId));
            }

            await _orderCommentsClient.CreateAsync(_comment, headers);
        }
    }
}
