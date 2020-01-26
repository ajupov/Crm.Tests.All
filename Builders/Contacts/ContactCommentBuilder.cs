using System;
using System.Threading.Tasks;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.v1.Clients.Contacts.Clients;
using Crm.v1.Clients.Contacts.Models;

namespace Crm.Tests.All.Builders.Contacts
{
    public class ContactCommentBuilder : IContactCommentBuilder
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly IContactCommentsClient _contactCommentsClient;
        private readonly ContactComment _comment;

        public ContactCommentBuilder(IAccessTokenGetter accessTokenGetter, IContactCommentsClient contactCommentsClient)
        {
            _contactCommentsClient = contactCommentsClient;
            _accessTokenGetter = accessTokenGetter;
            _comment = new ContactComment
            {
                Value = "Test".WithGuid()
            };
        }

        public ContactCommentBuilder WithContactId(Guid contactId)
        {
            _comment.ContactId = contactId;

            return this;
        }

        public async Task BuildAsync()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            if (_comment.ContactId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_comment.ContactId));
            }

            await _contactCommentsClient.CreateAsync(accessToken, _comment);
        }
    }
}