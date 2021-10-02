using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Tasks.Clients;
using Crm.v1.Clients.Tasks.Models;
using CrmTask = Crm.v1.Clients.Tasks.Models.Task;

namespace Crm.Tests.All.Builders.Tasks
{
    public class TaskBuilder : ITaskBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ITasksClient _tasksClient;
        private readonly CrmTask _task;

        public TaskBuilder(IDefaultRequestHeadersService defaultRequestHeadersService, ITasksClient tasksClient)
        {
            _tasksClient = tasksClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _task = new CrmTask
            {
                Id = Guid.NewGuid(),
                Name = "Test".WithGuid(),
                Description = "Test".WithGuid(),
                Result = "Test".WithGuid(),
                Priority = TaskPriority.Medium,
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddDays(1),
                DeadLineDateTime = null,
                IsDeleted = false
            };
        }

        public TaskBuilder WithTypeId(Guid typeId)
        {
            _task.TypeId = typeId;

            return this;
        }

        public TaskBuilder WithCustomerId(Guid customerId)
        {
            _task.CustomerId = customerId;

            return this;
        }

        public TaskBuilder WithStatusId(Guid statusId)
        {
            _task.StatusId = statusId;

            return this;
        }

        public TaskBuilder WithOrderId(Guid orderId)
        {
            _task.OrderId = orderId;

            return this;
        }

        public TaskBuilder WithResponsibleUserId(Guid responsibleUserId)
        {
            _task.ResponsibleUserId = responsibleUserId;

            return this;
        }

        public TaskBuilder WithName(string name)
        {
            _task.Name = name;

            return this;
        }

        public TaskBuilder WithDescription(string description)
        {
            _task.Description = description;

            return this;
        }

        public TaskBuilder WithResult(string result)
        {
            _task.Result = result;

            return this;
        }

        public TaskBuilder WithPriority(TaskPriority priority)
        {
            _task.Priority = priority;

            return this;
        }

        public TaskBuilder WithStartDateTime(DateTime startDateTime)
        {
            _task.StartDateTime = startDateTime;

            return this;
        }

        public TaskBuilder WithEndDateTime(DateTime endDateTime)
        {
            _task.EndDateTime = endDateTime;

            return this;
        }

        public TaskBuilder WithDeadLineDateTime(DateTime deadLineDateTime)
        {
            _task.DeadLineDateTime = deadLineDateTime;

            return this;
        }

        public TaskBuilder AsDeleted()
        {
            _task.IsDeleted = true;

            return this;
        }

        public TaskBuilder WithAttributeLink(Guid attributeId, string value)
        {
            if (_task.AttributeLinks == null)
            {
                _task.AttributeLinks = new List<TaskAttributeLink>();
            }

            _task.AttributeLinks.Add(new TaskAttributeLink
            {
                TaskAttributeId = attributeId,
                Value = value
            });

            return this;
        }

        public async Task<CrmTask> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            if (_task.TypeId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_task.TypeId));
            }

            if (_task.StatusId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_task.StatusId));
            }

            var id = await _tasksClient.CreateAsync(_task, headers);

            return await _tasksClient.GetAsync(id, headers);
        }
    }
}
