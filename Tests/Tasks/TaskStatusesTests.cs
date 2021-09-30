using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Tasks.Clients;
using Crm.v1.Clients.Tasks.Requests;
using Xunit;
using CrmTaskStatus = Crm.v1.Clients.Tasks.Models.TaskStatus;

namespace Crm.Tests.All.Tests.Tasks
{
    public class TaskStatusesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ITaskStatusesClient _taskStatusesClient;

        public TaskStatusesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ITaskStatusesClient taskStatusesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _taskStatusesClient = taskStatusesClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var statusId = (await _create.TaskStatus.BuildAsync()).Id;

            var status = await _taskStatusesClient.GetAsync(statusId, headers);

            Assert.NotNull(status);
            Assert.Equal(statusId, status.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var statusIds = (
                    await Task.WhenAll(
                        _create.TaskStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.TaskStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var statuses = await _taskStatusesClient.GetListAsync(statusIds, headers);

            Assert.NotEmpty(statuses);
            Assert.Equal(statusIds.Count, statuses.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var name = "Test".WithGuid();

            await Task.WhenAll(
                _create.TaskStatus
                    .WithName(name)
                    .BuildAsync());

            var request = new TaskStatusGetPagedListRequest
            {
                Name = name
            };

            var response = await _taskStatusesClient.GetPagedListAsync(request, headers);

            var results = response.Statuses
                .Skip(1)
                .Zip(response.Statuses, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Statuses);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = new CrmTaskStatus
            {
                Name = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdStatusId = await _taskStatusesClient.CreateAsync(status, headers);

            var createdStatus = await _taskStatusesClient.GetAsync(createdStatusId, headers);

            Assert.NotNull(createdStatus);
            Assert.Equal(createdStatusId, createdStatus.Id);
            Assert.Equal(status.Name, createdStatus.Name);
            Assert.Equal(status.IsDeleted, createdStatus.IsDeleted);
            Assert.True(createdStatus.CreateDateTime.IsMoreThanMinValue());
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = await _create.TaskStatus.WithName("Test".WithGuid()).BuildAsync();

            status.Name = "Test".WithGuid();
            status.IsDeleted = true;

            await _taskStatusesClient.UpdateAsync(status, headers);

            var updatedStatus = await _taskStatusesClient.GetAsync(status.Id, headers);

            Assert.Equal(status.Name, updatedStatus.Name);
            Assert.Equal(status.IsDeleted, updatedStatus.IsDeleted);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var statusIds = (
                    await Task.WhenAll(
                        _create.TaskStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.TaskStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _taskStatusesClient.DeleteAsync(statusIds, headers);

            var statuses = await _taskStatusesClient.GetListAsync(statusIds, headers);

            Assert.All(statuses, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var statusIds = (
                    await Task.WhenAll(
                        _create.TaskStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.TaskStatus
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _taskStatusesClient.RestoreAsync(statusIds, headers);

            var statuses = await _taskStatusesClient.GetListAsync(statusIds, headers);

            Assert.All(statuses, x => Assert.False(x.IsDeleted));
        }
    }
}
