using System;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Tasks.Clients;
using Crm.v1.Clients.Tasks.Models;
using Task = System.Threading.Tasks.Task;

namespace Crm.Tests.All.Builders.Tasks
{
    public class TaskCommentBuilder : ITaskCommentBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ITaskCommentsClient _taskCommentsClient;
        private readonly TaskComment _comment;

        public TaskCommentBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ITaskCommentsClient taskCommentsClient)
        {
            _taskCommentsClient = taskCommentsClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _comment = new TaskComment
            {
                Value = "Test".WithGuid()
            };
        }

        public TaskCommentBuilder WithTaskId(Guid taskId)
        {
            _comment.TaskId = taskId;

            return this;
        }

        public async Task BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            if (_comment.TaskId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_comment.TaskId));
            }

            await _taskCommentsClient.CreateAsync(_comment, headers);
        }
    }
}
