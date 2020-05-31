using System;
using System.Collections.Generic;
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
    public class ContactsTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IContactsClient _contactsClient;

        public ContactsTests(IAccessTokenGetter accessTokenGetter, ICreate create, IContactsClient contactsClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _contactsClient = contactsClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var leadSource = await _create.LeadSource.BuildAsync();
            var lead = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var contactId = (await _create.Contact.WithLeadId(lead.Id).BuildAsync()).Id;

            var contact = await _contactsClient.GetAsync(accessToken, contactId);

            Assert.NotNull(contact);
            Assert.Equal(contactId, contact.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var leadSource = await _create.LeadSource.BuildAsync();
            var lead1 = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var lead2 = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var contactIds = (
                    await Task.WhenAll(
                        _create.Contact
                            .WithLeadId(lead1.Id)
                            .WithTaxNumber("999999999999")
                            .BuildAsync(),
                        _create.Contact
                            .WithLeadId(lead2.Id)
                            .WithTaxNumber("999999999999")
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var contacts = await _contactsClient.GetListAsync(accessToken, contactIds);

            Assert.NotEmpty(contacts);
            Assert.Equal(contactIds.Count, contacts.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var leadSource = await _create.LeadSource.BuildAsync();
            var lead1 = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var lead2 = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var attribute = await _create.ContactAttribute.BuildAsync();
            var value = "Test".WithGuid();
            await Task.WhenAll(
                _create.Contact
                    .WithLeadId(lead1.Id)
                    .WithTaxNumber("999999999999")
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync(),
                _create.Contact
                    .WithLeadId(lead2.Id)
                    .WithTaxNumber("999999999999")
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync());
            var filterAttributes = new Dictionary<Guid, string> {{attribute.Id, value}};

            var request = new ContactGetPagedListRequest
            {
                Attributes = filterAttributes
            };

            var response = await _contactsClient.GetPagedListAsync(accessToken, request);

            var results = response.Contacts
                .Skip(1)
                .Zip(response.Contacts, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Contacts);
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
            var attribute = await _create.ContactAttribute.BuildAsync();

            var contact = new Contact
            {
                LeadId = lead.Id,
                Surname = "Test".WithGuid(),
                Name = "Test".WithGuid(),
                Patronymic = "Test".WithGuid(),
                Phone = "9999999999",
                Email = "test@test",
                TaxNumber = "999999999999",
                Post = "Test".WithGuid(),
                Postcode = "000000",
                Country = "Test".WithGuid(),
                Region = "Test".WithGuid(),
                Province = "Test".WithGuid(),
                City = "Test".WithGuid(),
                Street = "Test".WithGuid(),
                House = "1",
                Apartment = "1",
                BirthDate = new DateTime(1980, 1, 1),
                IsDeleted = false,
                AttributeLinks = new List<ContactAttributeLink>
                {
                    new ContactAttributeLink
                    {
                        ContactAttributeId = attribute.Id,
                        Value = "Test".WithGuid()
                    }
                },
                BankAccounts = new List<ContactBankAccount>
                {
                    new ContactBankAccount
                    {
                        Number = "9999999999999999999999999",
                        BankNumber = "9999999999",
                        BankCorrespondentNumber = "9999999999999999999999999",
                        BankName = "Test".WithGuid()
                    }
                }
            };

            var createdContactId = await _contactsClient.CreateAsync(accessToken, contact);

            var createdContact = await _contactsClient.GetAsync(accessToken, createdContactId);

            Assert.NotNull(createdContact);
            Assert.Equal(createdContactId, createdContact.Id);
            Assert.Equal(contact.LeadId, createdContact.LeadId);
            Assert.Equal(contact.CompanyId, createdContact.CompanyId);
            Assert.True(!createdContact.CreateUserId.IsEmpty());
            Assert.Equal(contact.ResponsibleUserId, createdContact.ResponsibleUserId);
            Assert.Equal(contact.Surname, createdContact.Surname);
            Assert.Equal(contact.Name, createdContact.Name);
            Assert.Equal(contact.Patronymic, createdContact.Patronymic);
            Assert.Equal(contact.Phone, createdContact.Phone);
            Assert.Equal(contact.Email, createdContact.Email);
            Assert.Equal(contact.TaxNumber, createdContact.TaxNumber);
            Assert.Equal(contact.Postcode, createdContact.Postcode);
            Assert.Equal(contact.Country, createdContact.Country);
            Assert.Equal(contact.Region, createdContact.Region);
            Assert.Equal(contact.Province, createdContact.Province);
            Assert.Equal(contact.City, createdContact.City);
            Assert.Equal(contact.Street, createdContact.Street);
            Assert.Equal(contact.House, createdContact.House);
            Assert.Equal(contact.Apartment, createdContact.Apartment);
            Assert.Equal(contact.BirthDate, createdContact.BirthDate);
            Assert.Equal(contact.IsDeleted, createdContact.IsDeleted);
            Assert.True(createdContact.CreateDateTime.IsMoreThanMinValue());
            Assert.NotEmpty(createdContact.AttributeLinks);
            Assert.NotEmpty(createdContact.BankAccounts);
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var leadSource = await _create.LeadSource.BuildAsync();
            var lead = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var attribute = await _create.ContactAttribute.BuildAsync();
            var contact = await _create.Contact
                .WithLeadId(lead.Id)
                .BuildAsync();

            contact.Surname = "Test".WithGuid();
            contact.Name = "Test".WithGuid();
            contact.Patronymic = "Test".WithGuid();
            contact.Phone = "9999999999";
            contact.Email = "test@test";
            contact.TaxNumber = "999999999999";
            contact.Post = "Test".WithGuid();
            contact.Postcode = "000000";
            contact.Country = "Test".WithGuid();
            contact.Region = "Test".WithGuid();
            contact.Province = "Test".WithGuid();
            contact.City = "Test".WithGuid();
            contact.Street = "Test".WithGuid();
            contact.House = "1";
            contact.Apartment = "1";
            contact.BirthDate = new DateTime(1990, 1, 1);
            contact.IsDeleted = true;
            contact.AttributeLinks.Add(
                new ContactAttributeLink
                {
                    ContactAttributeId = attribute.Id,
                    Value = "Test".WithGuid()
                });
            contact.BankAccounts.Add(
                new ContactBankAccount
                {
                    Number = "9999999999999999999999999",
                    BankNumber = "9999999999",
                    BankCorrespondentNumber = "9999999999999999999999999",
                    BankName = "Test".WithGuid()
                });
            await _contactsClient.UpdateAsync(accessToken, contact);

            var updatedContact = await _contactsClient.GetAsync(accessToken, contact.Id);

            Assert.Equal(contact.AccountId, updatedContact.AccountId);
            Assert.Equal(contact.LeadId, updatedContact.LeadId);
            Assert.Equal(contact.CreateUserId, updatedContact.CreateUserId);
            Assert.Equal(contact.ResponsibleUserId, updatedContact.ResponsibleUserId);
            Assert.Equal(contact.Surname, updatedContact.Surname);
            Assert.Equal(contact.Name, updatedContact.Name);
            Assert.Equal(contact.Patronymic, updatedContact.Patronymic);
            Assert.Equal(contact.Phone, updatedContact.Phone);
            Assert.Equal(contact.Email, updatedContact.Email);
            Assert.Equal(contact.TaxNumber, updatedContact.TaxNumber);
            Assert.Equal(contact.Post, updatedContact.Post);
            Assert.Equal(contact.Postcode, updatedContact.Postcode);
            Assert.Equal(contact.Country, updatedContact.Country);
            Assert.Equal(contact.Region, updatedContact.Region);
            Assert.Equal(contact.Province, updatedContact.Province);
            Assert.Equal(contact.City, updatedContact.City);
            Assert.Equal(contact.Street, updatedContact.Street);
            Assert.Equal(contact.House, updatedContact.House);
            Assert.Equal(contact.Apartment, updatedContact.Apartment);
            Assert.Equal(contact.BirthDate, updatedContact.BirthDate);
            Assert.Equal(contact.IsDeleted, updatedContact.IsDeleted);
            Assert.Equal(contact.AttributeLinks.Single().ContactAttributeId,
                updatedContact.AttributeLinks.Single().ContactAttributeId);
            Assert.Equal(contact.AttributeLinks.Single().Value, updatedContact.AttributeLinks.Single().Value);
            Assert.Equal(contact.BankAccounts.Single().Number, updatedContact.BankAccounts.Single().Number);
            Assert.Equal(contact.BankAccounts.Single().BankNumber, updatedContact.BankAccounts.Single().BankNumber);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var leadSource = await _create.LeadSource.BuildAsync();
            var lead1 = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var lead2 = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var contactIds = (
                    await Task.WhenAll(
                        _create.Contact
                            .WithLeadId(lead1.Id)
                            .WithTaxNumber("999999999999")
                            .BuildAsync(),
                        _create.Contact
                            .WithLeadId(lead2.Id)
                            .WithTaxNumber("999999999999")
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _contactsClient.DeleteAsync(accessToken, contactIds);

            var contacts = await _contactsClient.GetListAsync(accessToken, contactIds);

            Assert.All(contacts, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var leadSource = await _create.LeadSource.BuildAsync();
            var lead1 = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var lead2 = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var contactIds = (
                    await Task.WhenAll(
                        _create.Contact
                            .WithLeadId(lead1.Id)
                            .WithTaxNumber("999999999999")
                            .BuildAsync(),
                        _create.Contact
                            .WithLeadId(lead2.Id)
                            .WithTaxNumber("999999999999")
                            .BuildAsync())
                )
                .Select(x => x.Id).ToList();

            await _contactsClient.RestoreAsync(accessToken, contactIds);

            var contacts = await _contactsClient.GetListAsync(accessToken, contactIds);

            Assert.All(contacts, x => Assert.False(x.IsDeleted));
        }
    }
}
