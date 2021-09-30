using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Customers.Clients;
using Crm.v1.Clients.Customers.Models;
using Crm.v1.Clients.Customers.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Customers
{
    public class CustomerCommentsTests
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ICreate _create;
        private readonly ICustomerCommentsClient _customerCommentsClient;

        public CustomerCommentsTests(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ICreate create,
            ICustomerCommentsClient customerCommentsClient)
        {
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _create = create;
            _customerCommentsClient = customerCommentsClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var source = await _create.CustomerSource.BuildAsync();
            var customer = await _create.Customer
                .WithSourceId(source.Id)
                .BuildAsync();
            await Task.WhenAll(
                _create.CustomerComment
                    .WithCustomerId(customer.Id)
                    .BuildAsync(),
                _create.CustomerComment
                    .WithCustomerId(customer.Id)
                    .BuildAsync());

            var request = new CustomerCommentGetPagedListRequest
            {
                CustomerId = customer.Id
            };

            var response = await _customerCommentsClient.GetPagedListAsync(request, headers);

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

            var source = await _create.CustomerSource.BuildAsync();
            var customer = await _create.Customer
                .WithSourceId(source.Id)
                .BuildAsync();

            var comment = new CustomerComment
            {
                CustomerId = customer.Id,
                Value = "Test".WithGuid()
            };

            await _customerCommentsClient.CreateAsync(comment, headers);

            var request = new CustomerCommentGetPagedListRequest
            {
                CustomerId = customer.Id
            };

            var createdComment = (await _customerCommentsClient.GetPagedListAsync(request, headers)).Comments.First();

            Assert.NotNull(createdComment);
            Assert.Equal(comment.CustomerId, createdComment.CustomerId);
            Assert.True(!createdComment.CommentatorUserId.IsEmpty());
            Assert.Equal(comment.Value, createdComment.Value);
            Assert.True(createdComment.CreateDateTime.IsMoreThanMinValue());
        }
    }
}
