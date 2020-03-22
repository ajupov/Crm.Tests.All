using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Companies.Clients;
using Crm.v1.Clients.Companies.Models;
using Crm.v1.Clients.Companies.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Companies
{
    public class CompanyAttributeChangesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly ICompanyAttributesClient _companyAttributesClient;
        private readonly ICompanyAttributeChangesClient _attributeChangesClient;

        public CompanyAttributeChangesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            ICompanyAttributesClient companyAttributesClient,
            ICompanyAttributeChangesClient attributeChangesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _companyAttributesClient = companyAttributesClient;
            _attributeChangesClient = attributeChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attribute = await _create.CompanyAttribute.BuildAsync();

            attribute.Type = AttributeType.Link;
            attribute.Key = "Test".WithGuid();
            attribute.IsDeleted = true;

            await _companyAttributesClient.UpdateAsync(accessToken, attribute);

            var request = new CompanyAttributeChangeGetPagedListRequest
            {
                AttributeId = attribute.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _attributeChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.AttributeId == attribute.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<CompanyAttribute>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<CompanyAttribute>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<CompanyAttribute>().IsDeleted);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<CompanyAttribute>().Key, attribute.Key);
            Assert.Equal(response.Changes.Last().NewValueJson.FromJsonString<CompanyAttribute>().Type, attribute.Type);
        }
    }
}