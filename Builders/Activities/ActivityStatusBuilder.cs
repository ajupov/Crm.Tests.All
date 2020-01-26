using System.Threading.Tasks;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.v1.Clients.Activities.Clients;
using Crm.v1.Clients.Activities.Models;

namespace Crm.Tests.All.Builders.Activities
{
    public class ActivityStatusBuilder : IActivityStatusBuilder
    {
        private readonly IAccessTokenGetter _accessTokenGetter;

        private readonly IActivityStatusesClient _activityStatusesClient;
        private readonly ActivityStatus _status;

        public ActivityStatusBuilder(
            IAccessTokenGetter accessTokenGetter,
            IActivityStatusesClient activityStatusesClient)
        {
            _activityStatusesClient = activityStatusesClient;
            _accessTokenGetter = accessTokenGetter;
            _status = new ActivityStatus
            {
                Name = "Test".WithGuid(),
                IsFinish = false,
                IsDeleted = false
            };
        }

        public ActivityStatusBuilder WithName(string name)
        {
            _status.Name = name;

            return this;
        }

        public ActivityStatusBuilder AsFinish()
        {
            _status.IsFinish = true;

            return this;
        }

        public ActivityStatusBuilder AsDeleted()
        {
            _status.IsDeleted = true;

            return this;
        }

        public async Task<ActivityStatus> BuildAsync()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var id = await _activityStatusesClient.CreateAsync(accessToken, _status);

            return await _activityStatusesClient.GetAsync(accessToken, id);
        }
    }
}