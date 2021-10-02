using System;
using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Tasks.Clients;
using Crm.v1.Clients.Tasks.Models;

namespace Crm.Tests.All.Builders.Tasks
{
    public class TaskAttributeBuilder : ITaskAttributeBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ITaskAttributesClient _taskAttributesClient;
        private readonly TaskAttribute _attribute;

        public TaskAttributeBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ITaskAttributesClient taskAttributesClient)
        {
            _taskAttributesClient = taskAttributesClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _attribute = new TaskAttribute
            {
                Id = Guid.NewGuid(),
                Type = AttributeType.Text,
                Key = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public TaskAttributeBuilder WithType(AttributeType type)
        {
            _attribute.Type = type;

            return this;
        }

        public TaskAttributeBuilder WithKey(string key)
        {
            _attribute.Key = key;

            return this;
        }

        public TaskAttributeBuilder AsDeleted()
        {
            _attribute.IsDeleted = true;

            return this;
        }

        public async Task<TaskAttribute> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var id = await _taskAttributesClient.CreateAsync(_attribute, headers);

            return await _taskAttributesClient.GetAsync(id, headers);
        }
    }
}
