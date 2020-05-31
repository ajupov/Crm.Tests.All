using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.V1.Clients.Contacts.Clients;
using Crm.V1.Clients.Contacts.Models;
using Crm.V1.Clients.Contacts.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Contacts
{
    public class ContactCommentsTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IContactCommentsClient _contactCommentsClient;

        public ContactCommentsTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IContactCommentsClient contactCommentsClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _contactCommentsClient = contactCommentsClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var leadSource = await _create.LeadSource.BuildAsync();
            var lead = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var contact = await _create.Contact
                .WithLeadId(lead.Id)
                .BuildAsync();
            await Task.WhenAll(
                _create.ContactComment
                    .WithContactId(contact.Id)
                    .BuildAsync(),
                _create.ContactComment
                    .WithContactId(contact.Id)
                    .BuildAsync());

            var request = new ContactCommentGetPagedListRequest
            {
                ContactId = contact.Id
            };

            var response = await _contactCommentsClient.GetPagedListAsync(accessToken, request);

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
            var contact = await _create.Contact
                .WithLeadId(lead.Id)
                .BuildAsync();

            var comment = new ContactComment
            {
                ContactId = contact.Id,
                Value = "Test".WithGuid()
            };

            await _contactCommentsClient.CreateAsync(accessToken, comment);

            var request = new ContactCommentGetPagedListRequest
            {
                ContactId = contact.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var createdComment = (await _contactCommentsClient.GetPagedListAsync(accessToken, request)).Comments
                .First();

            Assert.NotNull(createdComment);
            Assert.Equal(comment.ContactId, createdComment.ContactId);
            Assert.True(!createdComment.CommentatorUserId.IsEmpty());
            Assert.Equal(comment.Value, createdComment.Value);
            Assert.True(createdComment.CreateDateTime.IsMoreThanMinValue());
        }
    }
}
