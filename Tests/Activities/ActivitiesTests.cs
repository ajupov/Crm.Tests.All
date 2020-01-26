using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Activities.Clients;
using Crm.v1.Clients.Activities.Models;
using Crm.v1.Clients.Activities.RequestParameters;
using Xunit;

namespace Crm.Tests.All.Tests.Activities
{
    public class ActivityTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IActivitiesClient _activitiesClient;

        public ActivityTests(IAccessTokenGetter accessTokenGetter, ICreate create, IActivitiesClient activitiesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _activitiesClient = activitiesClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.ActivityType.BuildAsync();
            var status = await _create.ActivityStatus.BuildAsync();
            var activityId = (
                    await _create.Activity
                        .WithTypeId(type.Id)
                        .WithStatusId(status.Id)
                        .BuildAsync())
                .Id;

            var activity = await _activitiesClient.GetAsync(accessToken, activityId);

            Assert.NotNull(activity);
            Assert.Equal(activityId, activity.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.ActivityType.BuildAsync();
            var status = await _create.ActivityStatus.BuildAsync();
            var activityIds = (
                    await Task.WhenAll(
                        _create.Activity
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync(),
                        _create.Activity
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var activities = await _activitiesClient.GetListAsync(accessToken, activityIds);

            Assert.NotEmpty(activities);
            Assert.Equal(activityIds.Count, activities.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attribute = await _create.ActivityAttribute.BuildAsync();
            var type = await _create.ActivityType.BuildAsync();
            var status = await _create.ActivityStatus.BuildAsync();
            var value = "Test".WithGuid();
            await Task.WhenAll(
                _create.Activity
                    .WithTypeId(type.Id)
                    .WithStatusId(status.Id)
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync(),
                _create.Activity
                    .WithTypeId(type.Id)
                    .WithStatusId(status.Id)
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync());
            var filterAttributes = new Dictionary<Guid, string> {{attribute.Id, value}};
            var filterStatusIds = new List<Guid> {status.Id};

            var request = new ActivityGetPagedListRequestParameter
            {
                AllAttributes = false,
                Attributes = filterAttributes,
                StatusIds = filterStatusIds
            };

            var activities = await _activitiesClient.GetPagedListAsync(accessToken, request);

            var results = activities
                .Skip(1)
                .Zip(activities, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(activities);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attribute = await _create.ActivityAttribute.BuildAsync();
            var type = await _create.ActivityType.BuildAsync();
            var activityStatus = await _create.ActivityStatus.BuildAsync();

            var activity = new Activity
            {
                TypeId = type.Id,
                StatusId = activityStatus.Id,
                Name = "Test".WithGuid(),
                Description = "Test".WithGuid(),
                Result = "Test".WithGuid(),
                Priority = ActivityPriority.Medium,
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddDays(1),
                DeadLineDateTime = DateTime.UtcNow.AddDays(2),
                IsDeleted = true,
                AttributeLinks = new List<ActivityAttributeLink>
                {
                    new ActivityAttributeLink
                    {
                        ActivityAttributeId = attribute.Id,
                        Value = "Test".WithGuid()
                    }
                }
            };

            var activityId = await _activitiesClient.CreateAsync(accessToken, activity);

            var createdActivity = await _activitiesClient.GetAsync(accessToken, activityId);

            Assert.NotNull(createdActivity);
            Assert.Equal(activityId, createdActivity.Id);
            Assert.Equal(activity.TypeId, createdActivity.TypeId);
            Assert.Equal(activity.StatusId, createdActivity.StatusId);
            Assert.Equal(activity.LeadId, createdActivity.LeadId);
            Assert.Equal(activity.CompanyId, createdActivity.CompanyId);
            Assert.Equal(activity.ContactId, createdActivity.ContactId);
            Assert.Equal(activity.DealId, createdActivity.DealId);
            Assert.True(!createdActivity.CreateUserId.IsEmpty());
            Assert.Equal(activity.ResponsibleUserId, createdActivity.ResponsibleUserId);
            Assert.Equal(activity.Name, createdActivity.Name);
            Assert.Equal(activity.Description, createdActivity.Description);
            Assert.Equal(activity.Result, createdActivity.Result);
            Assert.Equal(activity.Priority, createdActivity.Priority);
            Assert.Equal(activity.StartDateTime?.Date, createdActivity.StartDateTime?.Date);
            Assert.Equal(activity.EndDateTime?.Date, createdActivity.EndDateTime?.Date);
            Assert.Equal(activity.DeadLineDateTime?.Date, createdActivity.DeadLineDateTime?.Date);
            Assert.Equal(activity.IsDeleted, createdActivity.IsDeleted);
            Assert.True(createdActivity.CreateDateTime.IsMoreThanMinValue());
            Assert.NotEmpty(createdActivity.AttributeLinks);
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.ActivityType.BuildAsync();
            var activityStatus = await _create.ActivityStatus.BuildAsync();
            var attribute = await _create.ActivityAttribute.BuildAsync();
            var activity = await _create.Activity
                .WithTypeId(type.Id)
                .WithStatusId(activityStatus.Id)
                .BuildAsync();

            activity.Name = "Test".WithGuid();
            activity.Description = "Test".WithGuid();
            activity.Result = "Test".WithGuid();
            activity.Priority = ActivityPriority.Medium;
            activity.StartDateTime = DateTime.UtcNow;
            activity.EndDateTime = DateTime.UtcNow.AddDays(1);
            activity.DeadLineDateTime = DateTime.UtcNow.AddDays(1);
            activity.IsDeleted = true;
            activity.AttributeLinks = new List<ActivityAttributeLink>
            {
                new ActivityAttributeLink {ActivityAttributeId = attribute.Id, Value = "Test".WithGuid()}
            };

            await _activitiesClient.UpdateAsync(accessToken, activity);

            var updatedActivity = await _activitiesClient.GetAsync(accessToken, activity.Id);

            Assert.Equal(activity.AccountId, updatedActivity.AccountId);
            Assert.Equal(activity.TypeId, updatedActivity.TypeId);
            Assert.Equal(activity.StatusId, updatedActivity.StatusId);
            Assert.Equal(activity.LeadId, updatedActivity.LeadId);
            Assert.Equal(activity.CompanyId, updatedActivity.CompanyId);
            Assert.Equal(activity.ContactId, updatedActivity.ContactId);
            Assert.Equal(activity.DealId, updatedActivity.DealId);
            Assert.Equal(activity.ResponsibleUserId, updatedActivity.ResponsibleUserId);
            Assert.Equal(activity.Name, updatedActivity.Name);
            Assert.Equal(activity.Description, updatedActivity.Description);
            Assert.Equal(activity.Result, updatedActivity.Result);
            Assert.Equal(activity.Priority, updatedActivity.Priority);
            Assert.Equal(activity.StartDateTime?.Date, updatedActivity.StartDateTime?.Date);
            Assert.Equal(activity.EndDateTime?.Date, updatedActivity.EndDateTime?.Date);
            Assert.Equal(activity.DeadLineDateTime?.Date, updatedActivity.DeadLineDateTime?.Date);
            Assert.Equal(activity.IsDeleted, updatedActivity.IsDeleted);
            Assert.Equal(activity.AttributeLinks.Single().Value, updatedActivity.AttributeLinks.Single().Value);
            Assert.Equal(
                activity.AttributeLinks.Single().ActivityAttributeId,
                updatedActivity.AttributeLinks.Single().ActivityAttributeId);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.ActivityType.BuildAsync();
            var status = await _create.ActivityStatus.BuildAsync();
            var activityIds = (
                    await Task.WhenAll(
                        _create.Activity
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync(),
                        _create.Activity
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _activitiesClient.DeleteAsync(accessToken, activityIds);

            var activities = await _activitiesClient.GetListAsync(accessToken, activityIds);

            Assert.All(activities, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.ActivityType.BuildAsync();
            var status = await _create.ActivityStatus.BuildAsync();
            var activityIds = (
                    await Task.WhenAll(
                        _create.Activity
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync(),
                        _create.Activity
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _activitiesClient.RestoreAsync(accessToken, activityIds);

            var activities = await _activitiesClient.GetListAsync(accessToken, activityIds);

            Assert.All(activities, x => Assert.False(x.IsDeleted));
        }
    }
}