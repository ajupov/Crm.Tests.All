using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Suppliers.Clients;
using Crm.v1.Clients.Suppliers.Models;
using Xunit;

namespace Crm.Tests.All.Tests.Suppliers
{
    public class SupplierChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ISuppliersClient _suppliersClient;
        private readonly ISupplierChangesClient _supplierChangesClient;

        public SupplierChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ISuppliersClient suppliersClient,
            ISupplierChangesClient supplierChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _suppliersClient = suppliersClient;
            _supplierChangesClient = supplierChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var supplier = await _create.Supplier
                .BuildAsync();

            supplier.IsDeleted = true;

            await _suppliersClient.UpdateAsync(supplier, headers);

            var request = new SupplierChangeGetPagedListRequest
            {
                SupplierId = supplier.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _supplierChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.SupplierId == supplier.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<Supplier>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<Supplier>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<Supplier>().IsDeleted);
        }
    }
}
