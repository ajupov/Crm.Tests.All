using System;
using System.Threading.Tasks;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.V1.Clients.Deals.Clients;
using Crm.V1.Clients.Deals.Models;

namespace Crm.Tests.All.Builders.Deals
{
    public class DealCommentBuilder : IDealCommentBuilder
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly IDealCommentsClient _dealCommentsClient;
        private readonly DealComment _comment;

        public DealCommentBuilder(IAccessTokenGetter accessTokenGetter, IDealCommentsClient dealCommentsClient)
        {
            _dealCommentsClient = dealCommentsClient;
            _accessTokenGetter = accessTokenGetter;
            _comment = new DealComment
            {
                Value = "Test".WithGuid()
            };
        }

        public DealCommentBuilder WithDealId(Guid dealId)
        {
            _comment.DealId = dealId;

            return this;
        }

        public async Task BuildAsync()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            if (_comment.DealId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_comment.DealId));
            }

            await _dealCommentsClient.CreateAsync(accessToken, _comment);
        }
    }
}
