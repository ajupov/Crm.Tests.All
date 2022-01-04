using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Stock.Clients;
using Crm.v1.Clients.Stock.Models;
using Xunit;

namespace Crm.Tests.All.Tests.Stock
{
    public class StockArrivalChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IStockArrivalsClient _stockArrivalsClient;
        private readonly IStockArrivalChangesClient _stockArrivalChangesClient;

        public StockArrivalChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IStockArrivalsClient stockArrivalsClient,
            IStockArrivalChangesClient stockArrivalChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _stockArrivalsClient = stockArrivalsClient;
            _stockArrivalChangesClient = stockArrivalChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var productStatus = await _create.ProductStatus.BuildAsync();
            var room = await _create.StockRoom
                .BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var arrival = await _create.StockArrival
                .WithType(StockArrivalType.ArrivalFromSupplier)
                .WithItem(room.Id, product.Id, 1)
                .BuildAsync();

            arrival.IsDeleted = true;

            await _stockArrivalsClient.UpdateAsync(arrival, headers);

            var request = new StockArrivalChangeGetPagedListRequest
            {
                StockArrivalId = arrival.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _stockArrivalChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.StockArrivalId == arrival.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<StockArrival>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<StockArrival>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<StockArrival>().IsDeleted);
        }
    }
}
