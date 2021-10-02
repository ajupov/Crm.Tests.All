using System;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Orders.Clients;
using Crm.v1.Clients.Orders.Models;
using Crm.v1.Clients.Orders.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Orders
{
    public class OrderCommentsTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IOrderCommentsClient _orderCommentsClient;

        public OrderCommentsTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IOrderCommentsClient orderCommentsClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _orderCommentsClient = orderCommentsClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.OrderType.BuildAsync();
            var status = await _create.OrderStatus.BuildAsync();
            var order = await _create.Order
                .WithTypeId(type.Id)
                .WithStatusId(status.Id)
                .BuildAsync();
            await Task.WhenAll(
                _create.OrderComment
                    .WithOrderId(order.Id)
                    .BuildAsync(),
                _create.OrderComment
                    .WithOrderId(order.Id)
                    .BuildAsync());

            var request = new OrderCommentGetPagedListRequest
            {
                OrderId = order.Id,
            };

            var response = await _orderCommentsClient.GetPagedListAsync(request, headers);

            var results = response.Comments
                .Skip(1)
                .Zip(response.Comments, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Comments);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.OrderType.BuildAsync();
            var status = await _create.OrderStatus.BuildAsync();
            var order = await _create.Order
                .WithTypeId(type.Id)
                .WithStatusId(status.Id)
                .BuildAsync();

            var comment = new OrderComment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Value = "Test".WithGuid()
            };

            await _orderCommentsClient.CreateAsync(comment, headers);

            var request = new OrderCommentGetPagedListRequest
            {
                OrderId = order.Id
            };

            var createdComment = (await _orderCommentsClient.GetPagedListAsync(request, headers)).Comments.First();

            Assert.NotNull(createdComment);
            Assert.Equal(comment.OrderId, createdComment.OrderId);
            Assert.True(!createdComment.CommentatorUserId.IsEmpty());
            Assert.Equal(comment.Value, createdComment.Value);
            Assert.True(createdComment.CreateDateTime.IsMoreThanMinValue());
        }
    }
}
