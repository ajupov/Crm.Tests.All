using System.Linq;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Tasks.Clients;
using Crm.v1.Clients.Tasks.Models;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Crm.Tests.All.Tests.Tasks
{
    public class TaskTypeChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ITaskTypesClient _taskTypesClient;
        private readonly ITaskTypeChangesClient _typeChangesClient;

        public TaskTypeChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ITaskTypesClient taskTypesClient,
            ITaskTypeChangesClient typeChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _taskTypesClient = taskTypesClient;
            _typeChangesClient = typeChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.TaskType.BuildAsync();

            type.Name = "Test".WithGuid();
            type.IsDeleted = true;

            await _taskTypesClient.UpdateAsync(type, headers);

            var request = new TaskTypeChangeGetPagedListRequest
            {
                TypeId = type.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _typeChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.TypeId == type.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<TaskType>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<TaskType>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<TaskType>().IsDeleted);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<TaskType>().Name, type.Name);
        }
    }
}
