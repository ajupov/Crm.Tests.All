using System.Threading.Tasks;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Tasks.Clients;
using CrmTaskStatus = Crm.v1.Clients.Tasks.Models.TaskStatus;

namespace Crm.Tests.All.Builders.Tasks
{
    public class TaskStatusBuilder : ITaskStatusBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;

        private readonly ITaskStatusesClient _taskStatusesClient;
        private readonly CrmTaskStatus _status;

        public TaskStatusBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ITaskStatusesClient taskStatusesClient)
        {
            _taskStatusesClient = taskStatusesClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _status = new CrmTaskStatus
            {
                Name = "Test".WithGuid(),
                IsFinish = false,
                IsDeleted = false
            };
        }

        public TaskStatusBuilder WithName(string name)
        {
            _status.Name = name;

            return this;
        }

        public TaskStatusBuilder AsFinish()
        {
            _status.IsFinish = true;

            return this;
        }

        public TaskStatusBuilder AsDeleted()
        {
            _status.IsDeleted = true;

            return this;
        }

        public async Task<CrmTaskStatus> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var id = await _taskStatusesClient.CreateAsync(_status, headers);

            return await _taskStatusesClient.GetAsync(id, headers);
        }
    }
}
