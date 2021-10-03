using System;
using System.Linq;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Tasks.Clients;
using Crm.v1.Clients.Tasks.Models;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Crm.Tests.All.Tests.Tasks
{
    public class TaskCommentsTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ITaskCommentsClient _taskCommentsClient;

        public TaskCommentsTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ITaskCommentsClient taskCommentsClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _taskCommentsClient = taskCommentsClient;
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

            await Task.WhenAll(
                _create.TaskComment
                    .WithTaskId(task.Id)
                    .BuildAsync(),
                _create.TaskComment
                    .WithTaskId(task.Id)
                    .BuildAsync());

            var request = new TaskCommentGetPagedListRequest
            {
                TaskId = task.Id
            };

            var response = await _taskCommentsClient.GetPagedListAsync(request, headers);

            var results = response.Comments
                .Skip(1)
                .Zip(response.Comments, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Comments);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var type = await _create.TaskType.BuildAsync();
            var status = await _create.TaskStatus.BuildAsync();
            var task = await _create.Task
                .WithTypeId(type.Id)
                .WithStatusId(status.Id)
                .BuildAsync();

            var comment = new TaskComment
            {
                Id = Guid.NewGuid(),
                TaskId = task.Id,
                Value = "Test".WithGuid()
            };

            await _taskCommentsClient.CreateAsync(comment, headers);

            var request = new TaskCommentGetPagedListRequest
            {
                TaskId = task.Id
            };

            var createdComment = (await _taskCommentsClient.GetPagedListAsync(request, headers)).Comments
                .First();

            Assert.NotNull(createdComment);
            Assert.Equal(comment.TaskId, createdComment.TaskId);
            Assert.True(!createdComment.CommentatorUserId.IsEmpty());
            Assert.Equal(comment.Value, createdComment.Value);
            Assert.True(createdComment.CreateDateTime.IsMoreThanMinValue());
        }
    }
}
