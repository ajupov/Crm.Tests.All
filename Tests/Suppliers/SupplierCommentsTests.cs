using System;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Suppliers.Clients;
using Crm.v1.Clients.Suppliers.Models;
using Xunit;

namespace Crm.Tests.All.Tests.Suppliers
{
    public class SupplierCommentsTests
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ICreate _create;
        private readonly ISupplierCommentsClient _supplierCommentsClient;

        public SupplierCommentsTests(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ICreate create,
            ISupplierCommentsClient supplierCommentsClient)
        {
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _create = create;
            _supplierCommentsClient = supplierCommentsClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var supplier = await _create.Supplier
                .BuildAsync();
            await Task.WhenAll(
                _create.SupplierComment
                    .WithSupplierId(supplier.Id)
                    .BuildAsync(),
                _create.SupplierComment
                    .WithSupplierId(supplier.Id)
                    .BuildAsync());

            var request = new SupplierCommentGetPagedListRequest
            {
                SupplierId = supplier.Id
            };

            var response = await _supplierCommentsClient.GetPagedListAsync(request, headers);

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

            var supplier = await _create.Supplier
                .BuildAsync();

            var comment = new SupplierComment
            {
                Id = Guid.NewGuid(),
                SupplierId = supplier.Id,
                Value = "Test".WithGuid()
            };

            await _supplierCommentsClient.CreateAsync(comment, headers);

            var request = new SupplierCommentGetPagedListRequest
            {
                SupplierId = supplier.Id
            };

            var createdComment = (await _supplierCommentsClient.GetPagedListAsync(request, headers)).Comments.First();

            Assert.NotNull(createdComment);
            Assert.Equal(comment.SupplierId, createdComment.SupplierId);
            Assert.True(!createdComment.CommentatorUserId.IsEmpty());
            Assert.Equal(comment.Value, createdComment.Value);
            Assert.True(createdComment.CreateDateTime.IsMoreThanMinValue());
        }
    }
}
