using System;
using System.Linq;
using Ajupov.Utils.All.DateTime;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Tasks.Clients;
using Crm.v1.Clients.Tasks.Models;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Crm.Tests.All.Tests.Tasks
{
    public class TaskTypesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ITaskTypesClient _taskTypesClient;

        public TaskTypesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ITaskTypesClient taskTypesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _taskTypesClient = taskTypesClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var typeId = (await _create.TaskType.BuildAsync()).Id;

            var type = await _taskTypesClient.GetAsync(typeId, headers);

            Assert.NotNull(type);
            Assert.Equal(typeId, type.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var typeIds = (
                    await Task.WhenAll(
                        _create.TaskType
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.TaskType
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var types = await _taskTypesClient.GetListAsync(typeIds, headers);

            Assert.NotEmpty(types);
            Assert.Equal(typeIds.Count, types.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var name = "Test".WithGuid();
            await Task.WhenAll(_create.TaskType.WithName(name).BuildAsync());

            var request = new TaskTypeGetPagedListRequest
            {
                Name = name,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _taskTypesClient.GetPagedListAsync(request, headers);

            var results = response.Types
                .Skip(1)
                .Zip(response.Types, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Types);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = new TaskType
            {
                Id = Guid.NewGuid(),
                Name = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdTypeId = await _taskTypesClient.CreateAsync(type, headers);

            var createdType = await _taskTypesClient.GetAsync(createdTypeId, headers);

            Assert.NotNull(createdType);
            Assert.Equal(createdTypeId, createdType.Id);
            Assert.Equal(type.Name, createdType.Name);
            Assert.Equal(type.IsDeleted, createdType.IsDeleted);
            Assert.True(createdType.CreateDateTime.IsMoreThanMinValue());
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.TaskType.WithName("Test".WithGuid()).BuildAsync();

            type.Name = "Test".WithGuid();
            type.IsDeleted = true;

            await _taskTypesClient.UpdateAsync(type, headers);

            var updatedType = await _taskTypesClient.GetAsync(type.Id, headers);

            Assert.Equal(type.Name, updatedType.Name);
            Assert.Equal(type.IsDeleted, updatedType.IsDeleted);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var typeIds = (await Task.WhenAll(
                    _create.TaskType.WithName("Test".WithGuid()).BuildAsync(),
                    _create.TaskType.WithName("Test".WithGuid()).BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _taskTypesClient.DeleteAsync(typeIds, headers);

            var types = await _taskTypesClient.GetListAsync(typeIds, headers);

            Assert.All(types, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var typeIds = (
                    await Task.WhenAll(
                        _create.TaskType
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.TaskType
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _taskTypesClient.RestoreAsync(typeIds, headers);

            var types = await _taskTypesClient.GetListAsync(typeIds, headers);

            Assert.All(types, x => Assert.False(x.IsDeleted));
        }
    }
}
