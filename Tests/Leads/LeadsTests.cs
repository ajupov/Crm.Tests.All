using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.V1.Clients.Leads.Clients;
using Crm.V1.Clients.Leads.Models;
using Crm.V1.Clients.Leads.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Leads
{
    public class LeadsTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly ILeadsClient _leadsClient;

        public LeadsTests(IAccessTokenGetter accessTokenGetter, ICreate create, ILeadsClient leadsClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _leadsClient = leadsClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var source = await _create.LeadSource.BuildAsync();
            var leadId = (await _create.Lead.WithSourceId(source.Id).BuildAsync()).Id;

            var lead = await _leadsClient.GetAsync(accessToken, leadId);

            Assert.NotNull(lead);
            Assert.Equal(leadId, lead.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var source = await _create.LeadSource.BuildAsync();
            var leadIds = (
                    await Task.WhenAll(
                        _create.Lead
                            .WithSourceId(source.Id)
                            .BuildAsync(),
                        _create.Lead
                            .WithSourceId(source.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var leads = await _leadsClient.GetListAsync(accessToken, leadIds);

            Assert.NotEmpty(leads);
            Assert.Equal(leadIds.Count, leads.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attribute = await _create.LeadAttribute.BuildAsync();
            var value = "Test".WithGuid();
            var source = await _create.LeadSource
                .WithName(value)
                .BuildAsync();
            await Task.WhenAll(
                _create.Lead
                    .WithSourceId(source.Id)
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync(),
                _create.Lead
                    .WithSourceId(source.Id)
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync());
            var filterAttributes = new Dictionary<Guid, string> {{attribute.Id, value}};
            var filterSourceIds = new List<Guid> {source.Id};

            var request = new LeadGetPagedListRequest
            {
                Attributes = filterAttributes, SourceIds = filterSourceIds
            };

            var response = await _leadsClient.GetPagedListAsync(accessToken, request);

            var results = response.Leads
                .Skip(1)
                .Zip(response.Leads, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Leads);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attribute = await _create.LeadAttribute.BuildAsync();
            var source = await _create.LeadSource.BuildAsync();

            var lead = new Lead
            {
                SourceId = source.Id,
                Surname = "Test".WithGuid(),
                Name = "Test".WithGuid(),
                Patronymic = "Test".WithGuid(),
                Phone = "9999999999",
                Email = "test@test",
                CompanyName = "Test".WithGuid(),
                Post = "Test".WithGuid(),
                Postcode = "000000",
                Country = "Test".WithGuid(),
                Region = "Test".WithGuid(),
                Province = "Test".WithGuid(),
                City = "Test".WithGuid(),
                Street = "Test".WithGuid(),
                House = "1",
                Apartment = "1",
                OpportunitySum = 1,
                IsDeleted = true,
                AttributeLinks = new List<LeadAttributeLink>
                {
                    new LeadAttributeLink
                    {
                        LeadAttributeId = attribute.Id,
                        Value = "Test".WithGuid()
                    }
                }
            };

            var createdLeadId = await _leadsClient.CreateAsync(accessToken, lead);

            var createdLead = await _leadsClient.GetAsync(accessToken, createdLeadId);

            Assert.NotNull(createdLead);
            Assert.Equal(createdLeadId, createdLead.Id);
            Assert.Equal(lead.SourceId, createdLead.SourceId);
            Assert.True(!createdLead.CreateUserId.IsEmpty());
            Assert.Equal(lead.ResponsibleUserId, createdLead.ResponsibleUserId);
            Assert.Equal(lead.Surname, createdLead.Surname);
            Assert.Equal(lead.Name, createdLead.Name);
            Assert.Equal(lead.Patronymic, createdLead.Patronymic);
            Assert.Equal(lead.Phone, createdLead.Phone);
            Assert.Equal(lead.Email, createdLead.Email);
            Assert.Equal(lead.CompanyName, createdLead.CompanyName);
            Assert.Equal(lead.Post, createdLead.Post);
            Assert.Equal(lead.Postcode, createdLead.Postcode);
            Assert.Equal(lead.Country, createdLead.Country);
            Assert.Equal(lead.Region, createdLead.Region);
            Assert.Equal(lead.Province, createdLead.Province);
            Assert.Equal(lead.City, createdLead.City);
            Assert.Equal(lead.Street, createdLead.Street);
            Assert.Equal(lead.House, createdLead.House);
            Assert.Equal(lead.Apartment, createdLead.Apartment);
            Assert.Equal(lead.OpportunitySum, createdLead.OpportunitySum);
            Assert.Equal(lead.IsDeleted, createdLead.IsDeleted);
            Assert.True(createdLead.CreateDateTime.IsMoreThanMinValue());
            Assert.NotEmpty(createdLead.AttributeLinks);
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var source = await _create.LeadSource.BuildAsync();
            var attribute = await _create.LeadAttribute.BuildAsync();
            var lead = await _create.Lead
                .WithSourceId(source.Id)
                .BuildAsync();

            lead.SourceId = source.Id;
            lead.Surname = "Test".WithGuid();
            lead.Name = "Test".WithGuid();
            lead.Patronymic = "Test".WithGuid();
            lead.Phone = "9999999999";
            lead.Email = "test@test";
            lead.CompanyName = "Test".WithGuid();
            lead.Post = "Test".WithGuid();
            lead.Postcode = "000000";
            lead.Country = "Test".WithGuid();
            lead.Region = "Test".WithGuid();
            lead.Province = "Test".WithGuid();
            lead.City = "Test".WithGuid();
            lead.Street = "Test".WithGuid();
            lead.House = "1";
            lead.Apartment = "1";
            lead.OpportunitySum = 1;
            lead.IsDeleted = true;
            lead.AttributeLinks.Add(new LeadAttributeLink {LeadAttributeId = attribute.Id, Value = "Test".WithGuid()});
            await _leadsClient.UpdateAsync(accessToken, lead);

            var updatedLead = await _leadsClient.GetAsync(accessToken, lead.Id);

            Assert.Equal(lead.AccountId, updatedLead.AccountId);
            Assert.Equal(lead.SourceId, updatedLead.SourceId);
            Assert.Equal(lead.CreateUserId, updatedLead.CreateUserId);
            Assert.Equal(lead.ResponsibleUserId, updatedLead.ResponsibleUserId);
            Assert.Equal(lead.Surname, updatedLead.Surname);
            Assert.Equal(lead.Name, updatedLead.Name);
            Assert.Equal(lead.Patronymic, updatedLead.Patronymic);
            Assert.Equal(lead.Phone, updatedLead.Phone);
            Assert.Equal(lead.Email, updatedLead.Email);
            Assert.Equal(lead.CompanyName, updatedLead.CompanyName);
            Assert.Equal(lead.Post, updatedLead.Post);
            Assert.Equal(lead.Postcode, updatedLead.Postcode);
            Assert.Equal(lead.Country, updatedLead.Country);
            Assert.Equal(lead.Region, updatedLead.Region);
            Assert.Equal(lead.Province, updatedLead.Province);
            Assert.Equal(lead.City, updatedLead.City);
            Assert.Equal(lead.Street, updatedLead.Street);
            Assert.Equal(lead.House, updatedLead.House);
            Assert.Equal(lead.Apartment, updatedLead.Apartment);
            Assert.Equal(lead.OpportunitySum, updatedLead.OpportunitySum);
            Assert.Equal(lead.IsDeleted, updatedLead.IsDeleted);
            Assert.Equal(lead.AttributeLinks.Single().Value, updatedLead.AttributeLinks.Single().Value);
            Assert.Equal(
                lead.AttributeLinks.Single().LeadAttributeId,
                updatedLead.AttributeLinks.Single().LeadAttributeId);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var source = await _create.LeadSource.BuildAsync();
            var leadIds = (
                    await Task.WhenAll(
                        _create.Lead
                            .WithSourceId(source.Id)
                            .BuildAsync(),
                        _create.Lead
                            .WithSourceId(source.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _leadsClient.DeleteAsync(accessToken, leadIds);

            var leads = await _leadsClient.GetListAsync(accessToken, leadIds);

            Assert.All(leads, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var source = await _create.LeadSource.BuildAsync();
            var leadIds = (
                    await Task.WhenAll(
                        _create.Lead
                            .WithSourceId(source.Id)
                            .BuildAsync(),
                        _create.Lead
                            .WithSourceId(source.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _leadsClient.RestoreAsync(accessToken, leadIds);

            var leads = await _leadsClient.GetListAsync(accessToken, leadIds);

            Assert.All(leads, x => Assert.False(x.IsDeleted));
        }
    }
}
