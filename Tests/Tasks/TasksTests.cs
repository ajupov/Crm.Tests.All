using System;
using System.Collections.Generic;
using System.Linq;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
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
    public class TasksTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ITasksClient _tasksClient;

        public TasksTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ITasksClient tasksClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _tasksClient = tasksClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.TaskType.BuildAsync();
            var status = await _create.TaskStatus.BuildAsync();
            var taskId = (
                    await _create.Task
                        .WithTypeId(type.Id)
                        .WithStatusId(status.Id)
                        .BuildAsync())
                .Id;

            var task = await _tasksClient.GetAsync(taskId, headers);

            Assert.NotNull(task);
            Assert.Equal(taskId, task.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.TaskType.BuildAsync();
            var status = await _create.TaskStatus.BuildAsync();
            var taskIds = (
                    await Task.WhenAll(
                        _create.Task
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync(),
                        _create.Task
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var tasks = await _tasksClient.GetListAsync(taskIds, headers);

            Assert.NotEmpty(tasks);
            Assert.Equal(taskIds.Count, tasks.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = await _create.TaskAttribute.BuildAsync();
            var type = await _create.TaskType.BuildAsync();
            var status = await _create.TaskStatus.BuildAsync();
            var value = "Test".WithGuid();
            await Task.WhenAll(
                _create.Task
                    .WithTypeId(type.Id)
                    .WithStatusId(status.Id)
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync(),
                _create.Task
                    .WithTypeId(type.Id)
                    .WithStatusId(status.Id)
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync());
            var filterAttributes = new Dictionary<Guid, string> { { attribute.Id, value } };
            var filterStatusIds = new List<Guid> { status.Id };

            var request = new TaskGetPagedListRequest
            {
                AllAttributes = false,
                Attributes = filterAttributes,
                StatusIds = filterStatusIds
            };

            var response = await _tasksClient.GetPagedListAsync(request, headers);

            var results = response.Tasks
                .Skip(1)
                .Zip(response.Tasks, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Tasks);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = await _create.TaskAttribute.BuildAsync();
            var type = await _create.TaskType.BuildAsync();
            var taskStatus = await _create.TaskStatus.BuildAsync();

            var task = new CrmTask
            {
                Id = Guid.NewGuid(),
                TypeId = type.Id,
                StatusId = taskStatus.Id,
                Name = "Test".WithGuid(),
                Description = "Test".WithGuid(),
                Result = "Test".WithGuid(),
                Priority = TaskPriority.Medium,
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddDays(1),
                DeadLineDateTime = DateTime.UtcNow.AddDays(2),
                IsDeleted = true,
                AttributeLinks = new List<TaskAttributeLink>
                {
                    new ()
                    {
                        TaskAttributeId = attribute.Id,
                        Value = "Test".WithGuid()
                    }
                }
            };

            var taskId = await _tasksClient.CreateAsync(task, headers);

            var createdTask = await _tasksClient.GetAsync(taskId, headers);

            Assert.NotNull(createdTask);
            Assert.Equal(taskId, createdTask.Id);
            Assert.Equal(task.TypeId, createdTask.TypeId);
            Assert.Equal(task.StatusId, createdTask.StatusId);
            Assert.Equal(task.CustomerId, createdTask.CustomerId);
            Assert.Equal(task.OrderId, createdTask.OrderId);
            Assert.True(!createdTask.CreateUserId.IsEmpty());
            Assert.Equal(task.ResponsibleUserId, createdTask.ResponsibleUserId);
            Assert.Equal(task.Name, createdTask.Name);
            Assert.Equal(task.Description, createdTask.Description);
            Assert.Equal(task.Result, createdTask.Result);
            Assert.Equal(task.Priority, createdTask.Priority);
            Assert.Equal(task.StartDateTime?.Date, createdTask.StartDateTime?.Date);
            Assert.Equal(task.EndDateTime?.Date, createdTask.EndDateTime?.Date);
            Assert.Equal(task.DeadLineDateTime?.Date, createdTask.DeadLineDateTime?.Date);
            Assert.Equal(task.IsDeleted, createdTask.IsDeleted);
            Assert.True(createdTask.CreateDateTime.IsMoreThanMinValue());
            Assert.NotEmpty(createdTask.AttributeLinks);
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.TaskType.BuildAsync();
            var taskStatus = await _create.TaskStatus.BuildAsync();
            var attribute = await _create.TaskAttribute.BuildAsync();
            var task = await _create.Task
                .WithTypeId(type.Id)
                .WithStatusId(taskStatus.Id)
                .BuildAsync();

            task.Name = "Test".WithGuid();
            task.Description = "Test".WithGuid();
            task.Result = "Test".WithGuid();
            task.Priority = TaskPriority.Medium;
            task.StartDateTime = DateTime.UtcNow;
            task.EndDateTime = DateTime.UtcNow.AddDays(1);
            task.DeadLineDateTime = DateTime.UtcNow.AddDays(1);
            task.IsDeleted = true;
            task.AttributeLinks = new List<TaskAttributeLink>
            {
                new () { TaskAttributeId = attribute.Id, Value = "Test".WithGuid() }
            };

            await _tasksClient.UpdateAsync(task, headers);

            var updatedTask = await _tasksClient.GetAsync(task.Id, headers);

            Assert.Equal(task.AccountId, updatedTask.AccountId);
            Assert.Equal(task.TypeId, updatedTask.TypeId);
            Assert.Equal(task.StatusId, updatedTask.StatusId);
            Assert.Equal(task.CustomerId, updatedTask.CustomerId);
            Assert.Equal(task.OrderId, updatedTask.OrderId);
            Assert.Equal(task.ResponsibleUserId, updatedTask.ResponsibleUserId);
            Assert.Equal(task.Name, updatedTask.Name);
            Assert.Equal(task.Description, updatedTask.Description);
            Assert.Equal(task.Result, updatedTask.Result);
            Assert.Equal(task.Priority, updatedTask.Priority);
            Assert.Equal(task.StartDateTime?.Date, updatedTask.StartDateTime?.Date);
            Assert.Equal(task.EndDateTime?.Date, updatedTask.EndDateTime?.Date);
            Assert.Equal(task.DeadLineDateTime?.Date, updatedTask.DeadLineDateTime?.Date);
            Assert.Equal(task.IsDeleted, updatedTask.IsDeleted);
            Assert.Equal(task.AttributeLinks.Single().Value, updatedTask.AttributeLinks.Single().Value);
            Assert.Equal(
                task.AttributeLinks.Single().TaskAttributeId,
                updatedTask.AttributeLinks.Single().TaskAttributeId);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.TaskType.BuildAsync();
            var status = await _create.TaskStatus.BuildAsync();
            var taskIds = (
                    await Task.WhenAll(
                        _create.Task
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync(),
                        _create.Task
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _tasksClient.DeleteAsync(taskIds, headers);

            var tasks = await _tasksClient.GetListAsync(taskIds, headers);

            Assert.All(tasks, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.TaskType.BuildAsync();
            var status = await _create.TaskStatus.BuildAsync();
            var taskIds = (
                    await Task.WhenAll(
                        _create.Task
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync(),
                        _create.Task
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _tasksClient.RestoreAsync(taskIds, headers);

            var tasks = await _tasksClient.GetListAsync(taskIds, headers);

            Assert.All(tasks, x => Assert.False(x.IsDeleted));
        }
    }
}
