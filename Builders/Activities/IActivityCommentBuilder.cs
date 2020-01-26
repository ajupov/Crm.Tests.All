using System;
using System.Threading.Tasks;

namespace Crm.Tests.All.Builders.Activities
{
    public interface IActivityCommentBuilder
    {
        ActivityCommentBuilder WithActivityId(Guid activityId);

        Task BuildAsync();
    }
}