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
using CrmTask = Crm.v1.Clients.Tasks.Models.Task;
using Task = System.Threading.Tasks.Task;

namespace Crm.Tests.All.Tests.Tasks
{
    public class TaskChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ITasksClient _tasksClient;
        private readonly ITaskChangesClient _taskChangesClient;

        public TaskChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ITasksClient tasksClient,
            ITaskChangesClient taskChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _tasksClient = tasksClient;
            _taskChangesClient = taskChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.TaskType.BuildAsync();
            var status = await _create.TaskStatus.BuildAsync();
            var task = await _create.Task
                .WithTypeId(type.Id)
                .WithStatusId(status.Id)
                .BuildAsync();

            task.Name = "Test".WithGuid();
            task.IsDeleted = true;

            await _tasksClient.UpdateAsync(task, headers);

            var request = new TaskChangeGetPagedListRequest
            {
                TaskId = task.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _taskChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.TaskId == task.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<CrmTask>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<CrmTask>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<CrmTask>().IsDeleted);
        }
    }
}
