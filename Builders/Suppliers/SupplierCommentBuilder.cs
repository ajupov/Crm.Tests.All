using System;
using System.Threading.Tasks;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Suppliers.Clients;
using Crm.v1.Clients.Suppliers.Models;

namespace Crm.Tests.All.Builders.Suppliers
{
    public class SupplierCommentBuilder : ISupplierCommentBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ISupplierCommentsClient _customerCommentsClient;
        private readonly SupplierComment _comment;

        public SupplierCommentBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ISupplierCommentsClient customerCommentsClient)
        {
            _customerCommentsClient = customerCommentsClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _comment = new SupplierComment
            {
                Id = Guid.NewGuid(),
                Value = "Test".WithGuid()
            };
        }

        public SupplierCommentBuilder WithSupplierId(Guid customerId)
        {
            _comment.SupplierId = customerId;

            return this;
        }

        public async Task BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            if (_comment.SupplierId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_comment.SupplierId));
            }

            await _customerCommentsClient.CreateAsync(_comment, headers);
        }
    }
}
