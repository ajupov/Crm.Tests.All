using System;
using System.Threading.Tasks;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.V1.Clients.Companies.Clients;
using Crm.V1.Clients.Companies.Models;

namespace Crm.Tests.All.Builders.Companies
{
    public class CompanyCommentBuilder : ICompanyCommentBuilder
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICompanyCommentsClient _companyCommentsClient;
        private readonly CompanyComment _comment;

        public CompanyCommentBuilder(IAccessTokenGetter accessTokenGetter, ICompanyCommentsClient companyCommentsClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _companyCommentsClient = companyCommentsClient;
            _comment = new CompanyComment
            {
                Value = "Test".WithGuid()
            };
        }

        public CompanyCommentBuilder WithCompanyId(Guid companyId)
        {
            _comment.CompanyId = companyId;

            return this;
        }

        public async Task BuildAsync()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            if (_comment.CompanyId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_comment.CompanyId));
            }

            await _companyCommentsClient.CreateAsync(accessToken, _comment);
        }
    }
}
