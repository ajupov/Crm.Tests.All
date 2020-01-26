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
using Crm.v1.Clients.Companies.RequestParameters;
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

            var request = new CompanyChangeGetPagedListRequestParameter
            {
                CompanyId = company.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var changes = await _companyChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(changes);
            Assert.True(changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(changes.All(x => x.CompanyId == company.Id));
            Assert.True(changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(changes.First().OldValueJson.IsEmpty());
            Assert.True(!changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(changes.First().NewValueJson.FromJsonString<Company>());
            Assert.True(!changes.Last().OldValueJson.IsEmpty());
            Assert.True(!changes.Last().NewValueJson.IsEmpty());
            Assert.False(changes.Last().OldValueJson.FromJsonString<Company>().IsDeleted);
            Assert.True(changes.Last().NewValueJson.FromJsonString<Company>().IsDeleted);
        }
    }
}