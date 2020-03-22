using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Companies.Clients;
using Crm.v1.Clients.Companies.Models;
using Crm.v1.Clients.Companies.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Companies
{
    public class CompanyChangesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly ICompaniesClient _companiesClient;
        private readonly ICompanyChangesClient _companyChangesClient;

        public CompanyChangesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            ICompaniesClient companiesClient,
            ICompanyChangesClient companyChangesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _companiesClient = companiesClient;
            _companyChangesClient = companyChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var leadSource = await _create.LeadSource.BuildAsync();
            var lead = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var company = await _create.Company
                .WithLeadId(lead.Id)
                .BuildAsync();

            company.IsDeleted = true;

            await _companiesClient.UpdateAsync(accessToken, company);

            var request = new CompanyChangeGetPagedListRequest
            {
                CompanyId = company.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _companyChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.CompanyId == company.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<Company>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<Company>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<Company>().IsDeleted);
        }
    }
}