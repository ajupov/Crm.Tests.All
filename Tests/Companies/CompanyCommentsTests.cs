using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Companies.Clients;
using Crm.v1.Clients.Companies.Models;
using Crm.v1.Clients.Companies.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Companies
{
    public class CompanyCommentsTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly ICompanyCommentsClient _companyCommentsClient;

        public CompanyCommentsTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            ICompanyCommentsClient companyCommentsClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _companyCommentsClient = companyCommentsClient;
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

            await Task.WhenAll(
                _create.CompanyComment
                    .WithCompanyId(company.Id)
                    .BuildAsync(),
                _create.CompanyComment
                    .WithCompanyId(company.Id)
                    .BuildAsync());

            var request = new CompanyCommentGetPagedListRequest
            {
                CompanyId = company.Id
            };

            var response = await _companyCommentsClient.GetPagedListAsync(accessToken, request);
            var results = response.Comments
                .Skip(1)
                .Zip(response.Comments, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Comments);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var leadSource = await _create.LeadSource.BuildAsync();
            var lead = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var company = await _create.Company
                .WithLeadId(lead.Id)
                .BuildAsync();

            var comment = new CompanyComment
            {
                CompanyId = company.Id,
                Value = "Test".WithGuid()
            };

            await _companyCommentsClient.CreateAsync(accessToken, comment);

            var request = new CompanyCommentGetPagedListRequest
            {
                CompanyId = company.Id
            };

            var createdComment = (await _companyCommentsClient.GetPagedListAsync(accessToken, request)).Comments
                .First();

            Assert.NotNull(createdComment);
            Assert.Equal(comment.CompanyId, createdComment.CompanyId);
            Assert.True(!createdComment.CommentatorUserId.IsEmpty());
            Assert.Equal(comment.Value, createdComment.Value);
            Assert.True(createdComment.CreateDateTime.IsMoreThanMinValue());
        }
    }
}