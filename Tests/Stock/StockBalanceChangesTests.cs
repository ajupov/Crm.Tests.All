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
    public class StockBalanceChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IStockBalancesClient _stockBalancesClient;
        private readonly IStockBalanceChangesClient _stockBalanceChangesClient;

        public StockBalanceChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IStockBalancesClient stockBalancesClient,
            IStockBalanceChangesClient stockBalanceChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _stockBalancesClient = stockBalancesClient;
            _stockBalanceChangesClient = stockBalanceChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var room = await _create.StockRoom.BuildAsync();
            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var balance = await _create.StockBalance
                .WithRoomId(room.Id)
                .WithProductId(product.Id)
                .WithCount(1)
                .BuildAsync();

            balance.IsDeleted = true;

            await _stockBalancesClient.UpdateAsync(balance, headers);

            var request = new StockBalanceChangeGetPagedListRequest
            {
                StockBalanceId = balance.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _stockBalanceChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.StockBalanceId == balance.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<StockBalance>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<StockBalance>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<StockBalance>().IsDeleted);
        }
    }
}
