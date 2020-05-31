using System;
using System.Threading.Tasks;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.V1.Clients.Leads.Clients;
using Crm.V1.Clients.Leads.Models;

namespace Crm.Tests.All.Builders.Leads
{
    public class LeadCommentBuilder : ILeadCommentBuilder
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ILeadCommentsClient _leadCommentsClient;
        private readonly LeadComment _comment;

        public LeadCommentBuilder(IAccessTokenGetter accessTokenGetter, ILeadCommentsClient leadCommentsClient)
        {
            _leadCommentsClient = leadCommentsClient;
            _accessTokenGetter = accessTokenGetter;
            _comment = new LeadComment
            {
                Value = "Test".WithGuid()
            };
        }

        public LeadCommentBuilder WithLeadId(Guid leadId)
        {
            _comment.LeadId = leadId;

            return this;
        }

        public async Task BuildAsync()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            if (_comment.LeadId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_comment.LeadId));
            }

            await _leadCommentsClient.CreateAsync(accessToken, _comment);
        }
    }
}
