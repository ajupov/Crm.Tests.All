using System.Linq;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Tasks.Clients;
using Crm.v1.Clients.Tasks.Models;
using Crm.v1.Clients.Tasks.Requests;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Crm.Tests.All.Tests.Tasks
{
    public class TaskAttributeChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ITaskAttributesClient _taskAttributesClient;
        private readonly ITaskAttributeChangesClient _attributeChangesClient;

        public TaskAttributeChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ITaskAttributesClient taskAttributesClient,
            ITaskAttributeChangesClient attributeChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _taskAttributesClient = taskAttributesClient;
            _attributeChangesClient = attributeChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = await _create.TaskAttribute.BuildAsync();

            attribute.Type = AttributeType.Link;
            attribute.Key = "Test".WithGuid();
            attribute.IsDeleted = true;

            await _taskAttributesClient.UpdateAsync(attribute, headers);

            var request = new TaskAttributeChangeGetPagedListRequest
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
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<TaskAttribute>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<TaskAttribute>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<TaskAttribute>().IsDeleted);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<TaskAttribute>().Key, attribute.Key);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<TaskAttribute>().Type, attribute.Type);
        }
    }
}
