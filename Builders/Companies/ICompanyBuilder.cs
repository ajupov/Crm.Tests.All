using System;
using System.Threading.Tasks;
using Crm.V1.Clients.Companies.Models;

namespace Crm.Tests.All.Builders.Companies
{
    public interface ICompanyBuilder
    {
        CompanyBuilder WithType(CompanyType type);

        CompanyBuilder WithIndustryType(CompanyIndustryType type);

        CompanyBuilder WithLeadId(Guid leadId);

        CompanyBuilder WithCreateUserId(Guid createUserId);

        CompanyBuilder WithResponsibleUserId(Guid responsibleUserId);

        CompanyBuilder WithFullName(string fullName);

        CompanyBuilder WithShortName(string shortName);

        CompanyBuilder WithPhone(string phone);

        CompanyBuilder WithEmail(string email);

        CompanyBuilder WithTaxNumber(string taxNumber);

        CompanyBuilder WithRegistrationNumber(string registrationNumber);

        CompanyBuilder WithRegistrationDate(DateTime registrationDate);

        CompanyBuilder WithEmployeesCount(int employeesCount);

        CompanyBuilder WithYearlyTurnover(int yearlyTurnover);

        CompanyBuilder WithPostcode(string postcode);

        CompanyBuilder WithJuridicalCountry(string juridicalCountry);

        CompanyBuilder WithJuridicalRegion(string juridicalRegion);

        CompanyBuilder WithJuridicalProvince(string juridicalProvince);

        CompanyBuilder WithJuridicalCity(string juridicalCity);

        CompanyBuilder WithJuridicalStreet(string juridicalStreet);

        CompanyBuilder WithJuridicalHouse(string juridicalHouse);

        CompanyBuilder WithJuridicalApartment(string juridicalApartment);

        CompanyBuilder WithLegalCountry(string legalCountry);

        CompanyBuilder WithLegalRegion(string legalRegion);

        CompanyBuilder WithLegalProvince(string legalProvince);

        CompanyBuilder WithLegalCity(string legalCity);

        CompanyBuilder WithLegalStreet(string legalStreet);

        CompanyBuilder WithLegalHouse(string legalHouse);

        CompanyBuilder WithLegalApartment(string legalApartment);

        CompanyBuilder AsDeleted();

        CompanyBuilder WithBankAccount(
            string number,
            string bankNumber,
            string bankName,
            string bankCorrespondentNumber);

        CompanyBuilder WithAttributeLink(Guid attributeId, string value);

        Task<Company> BuildAsync();
    }
}
