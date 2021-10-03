using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Orders.Clients;
using Crm.v1.Clients.Orders.Models;
using Xunit;

namespace Crm.Tests.All.Tests.Orders
{
    public class OrderAttributeChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IOrderAttributesClient _orderAttributesClient;
        private readonly IOrderAttributeChangesClient _attributeChangesClient;

        public OrderAttributeChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IOrderAttributesClient orderAttributesClient,
            IOrderAttributeChangesClient attributeChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _orderAttributesClient = orderAttributesClient;
            _attributeChangesClient = attributeChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = await _create.OrderAttribute.BuildAsync();
            attribute.Type = AttributeType.Link;
            attribute.Key = "Test".WithGuid();
            attribute.IsDeleted = true;
            await _orderAttributesClient.UpdateAsync(attribute, headers);

            var request = new OrderAttributeChangeGetPagedListRequest
            {
                AttributeId = attribute.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _attributeChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.AttributeId == attribute.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<OrderAttribute>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<OrderAttribute>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<OrderAttribute>().IsDeleted);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<OrderAttribute>().Key, attribute.Key);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<OrderAttribute>().Type, attribute.Type);
        }
    }
}
