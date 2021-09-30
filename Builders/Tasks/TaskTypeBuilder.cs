using System.Threading.Tasks;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Tasks.Clients;
using Crm.v1.Clients.Tasks.Models;

namespace Crm.Tests.All.Builders.Tasks
{
    public class TaskTypeBuilder : ITaskTypeBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ITaskTypesClient _taskTypesClient;
        private readonly TaskType _type;

        public TaskTypeBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ITaskTypesClient taskTypesClient)
        {
            _taskTypesClient = taskTypesClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _type = new TaskType
            {
                Name = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public TaskTypeBuilder WithName(string name)
        {
            _type.Name = name;

            return this;
        }

        public TaskTypeBuilder AsDeleted()
        {
            _type.IsDeleted = true;

            return this;
        }

        public async Task<TaskType> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var id = await _taskTypesClient.CreateAsync(_type, headers);

            return await _taskTypesClient.GetAsync(id, headers);
        }
    }
}
