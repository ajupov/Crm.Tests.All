using System;
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
    public class StockConsumptionChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IStockConsumptionsClient _stockConsumptionsClient;
        private readonly IStockConsumptionChangesClient _stockConsumptionChangesClient;

        public StockConsumptionChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IStockConsumptionsClient stockConsumptionsClient,
            IStockConsumptionChangesClient stockConsumptionChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _stockConsumptionsClient = stockConsumptionsClient;
            _stockConsumptionChangesClient = stockConsumptionChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var consumption = await _create.StockConsumption
                .WithType(StockConsumptionType.SaleToCustomer)
                .WithOrderId(Guid.NewGuid())
                .WithItem(product.Id, 1)
                .BuildAsync();

            consumption.IsDeleted = true;

            await _stockConsumptionsClient.UpdateAsync(consumption, headers);

            var request = new StockConsumptionChangeGetPagedListRequest
            {
                StockConsumptionId = consumption.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _stockConsumptionChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.StockConsumptionId == consumption.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<StockConsumption>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<StockConsumption>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<StockConsumption>().IsDeleted);
        }
    }
}
