using System;
using System.Threading.Tasks;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Customers.Clients;
using Crm.v1.Clients.Customers.Models;

namespace Crm.Tests.All.Builders.Customers
{
    public class CustomerCommentBuilder : ICustomerCommentBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ICustomerCommentsClient _customerCommentsClient;
        private readonly CustomerComment _comment;

        public CustomerCommentBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ICustomerCommentsClient customerCommentsClient)
        {
            _customerCommentsClient = customerCommentsClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _comment = new CustomerComment
            {
                Value = "Test".WithGuid()
            };
        }

        public CustomerCommentBuilder WithCustomerId(Guid customerId)
        {
            _comment.CustomerId = customerId;

            return this;
        }

        public async Task BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            if (_comment.CustomerId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_comment.CustomerId));
            }

            await _customerCommentsClient.CreateAsync(_comment, headers);
        }
    }
}
