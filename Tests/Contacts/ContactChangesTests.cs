using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Ajupov.Utils.All.Json;
using Ajupov.Utils.All.String;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Contacts.Clients;
using Crm.v1.Clients.Contacts.Models;
using Crm.v1.Clients.Contacts.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Contacts
{
    public class ContactChangesTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IContactsClient _contactsClient;
        private readonly IContactChangesClient _contactChangesClient;

        public ContactChangesTests(
            IAccessTokenGetter accessTokenGetter,
            ICreate create,
            IContactsClient contactsClient,
            IContactChangesClient contactChangesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _contactsClient = contactsClient;
            _contactChangesClient = contactChangesClient;
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var leadSource = await _create.LeadSource.BuildAsync();
            var lead = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var contact = await _create.Contact.WithLeadId(lead.Id).BuildAsync();

            contact.IsDeleted = true;

            await _contactsClient.UpdateAsync(accessToken, contact);

            var request = new ContactChangeGetPagedListRequest
            {
                ContactId = contact.Id,
                SortBy = "CreateDateTime",
                OrderBy = "asc"
            };

            var response = await _contactChangesClient.GetPagedListAsync(accessToken, request);

            Assert.NotEmpty(response.Changes);
            Assert.True(response.Changes.All(x => !x.ChangerUserId.IsEmpty()));
            Assert.True(response.Changes.All(x => x.ContactId == contact.Id));
            Assert.True(response.Changes.All(x => x.CreateDateTime.IsMoreThanMinValue()));
            Assert.True(response.Changes.First().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.First().NewValueJson.IsEmpty());
            Assert.NotNull(response.Changes.First().NewValueJson.FromJsonString<Contact>());
            Assert.True(!response.Changes.Last().OldValueJson.IsEmpty());
            Assert.True(!response.Changes.Last().NewValueJson.IsEmpty());
            Assert.False(response.Changes.Last().OldValueJson.FromJsonString<Contact>().IsDeleted);
            Assert.True(response.Changes.Last().NewValueJson.FromJsonString<Contact>().IsDeleted);
        }
    }
}