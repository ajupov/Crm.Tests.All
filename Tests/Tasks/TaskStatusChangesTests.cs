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
using CrmTaskStatus = Crm.v1.Clients.Tasks.Models.TaskStatus;
using Task = System.Threading.Tasks.Task;

namespace Crm.Tests.All.Tests.Tasks
{
    public class TaskStatusChangesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ITaskStatusesClient _taskStatusesClient;
        private readonly ITaskStatusChangesClient _statusChangesClient;

        public TaskStatusChangesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ITaskStatusesClient taskStatusesClient,
            ITaskStatusChangesClient statusChangesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _taskStatusesClient = taskStatusesClient;
            _statusChangesClient = statusChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = await _create.TaskStatus.BuildAsync();

            status.Name = "Test".WithGuid();
            status.IsDeleted = true;

            await _taskStatusesClient.UpdateAsync(status, headers);

            var request = new TaskStatusChangeGetPagedListRequest
            {
                StatusId = status.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _statusChangesClient.GetPagedListAsync(request, headers);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.StatusId == status.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<CrmTaskStatus>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<CrmTaskStatus>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<CrmTaskStatus>().IsDeleted);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<CrmTaskStatus>().Name, status.Name);
        }
    }
}
