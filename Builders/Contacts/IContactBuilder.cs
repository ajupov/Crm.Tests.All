using System;
using System.Threading.Tasks;
using Crm.V1.Clients.Contacts.Models;

namespace Crm.Tests.All.Builders.Contacts
{
    public interface IContactBuilder
    {
        ContactBuilder WithLeadId(Guid leadId);

        ContactBuilder WithCompanyId(Guid companyId);

        ContactBuilder WithCreateUserId(Guid createUserId);

        ContactBuilder WithResponsibleUserId(Guid responsibleUserId);

        ContactBuilder WithSurname(string surname);

        ContactBuilder WithName(string name);

        ContactBuilder WithPatronymic(string patronymic);

        ContactBuilder WithPhone(string phone);

        ContactBuilder WithEmail(string email);

        ContactBuilder WithTaxNumber(string taxNumber);

        ContactBuilder WithPost(string post);

        ContactBuilder WithPostcode(string postcode);

        ContactBuilder WithCountry(string country);

        ContactBuilder WithRegion(string region);

        ContactBuilder WithProvince(string province);

        ContactBuilder WithCity(string city);

        ContactBuilder WithStreet(string street);

        ContactBuilder WithHouse(string house);

        ContactBuilder WithApartment(string apartment);

        ContactBuilder WithBirthDate(DateTime birthDate);

        ContactBuilder AsDeleted();

        ContactBuilder WithAttributeLink(Guid attributeId, string value);

        ContactBuilder WithBankAccount(
            string number,
            string bankNumber,
            string bankName,
            string bankCorrespondentNumber);

        Task<Contact> BuildAsync();
    }
}
