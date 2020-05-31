using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.V1.Clients.Companies.Clients;
using Crm.V1.Clients.Companies.Models;

namespace Crm.Tests.All.Builders.Companies
{
    public class CompanyBuilder : ICompanyBuilder
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICompaniesClient _companiesClient;
        private readonly Company _company;

        public CompanyBuilder(IAccessTokenGetter accessTokenGetter, ICompaniesClient companiesClient)
        {
            _companiesClient = companiesClient;
            _accessTokenGetter = accessTokenGetter;
            _company = new Company
            {
                Type = CompanyType.Commercial,
                IndustryType = CompanyIndustryType.Computer,
                FullName = "Test".WithGuid(),
                ShortName = "Test".WithGuid(),
                Phone = "9999999999",
                Email = "test@test",
                TaxNumber = "999999999999".WithGuid(),
                RegistrationNumber = "999999999999999",
                RegistrationDate = new DateTime(2000, 1, 1),
                EmployeesCount = 1,
                YearlyTurnover = 1000000,
                JuridicalPostcode = "000000",
                JuridicalCountry = "Test".WithGuid(),
                JuridicalRegion = "Test".WithGuid(),
                JuridicalProvince = "Test".WithGuid(),
                JuridicalCity = "Test".WithGuid(),
                JuridicalStreet = "Test".WithGuid(),
                JuridicalHouse = "1",
                JuridicalApartment = "1",
                LegalPostcode = "000000",
                LegalCountry = "Test".WithGuid(),
                LegalRegion = "Test".WithGuid(),
                LegalProvince = "Test".WithGuid(),
                LegalCity = "Test".WithGuid(),
                LegalStreet = "Test".WithGuid(),
                LegalHouse = "1",
                LegalApartment = "1",
                IsDeleted = false
            };
        }

        public CompanyBuilder WithType(CompanyType type)
        {
            _company.Type = type;

            return this;
        }

        public CompanyBuilder WithIndustryType(CompanyIndustryType type)
        {
            _company.IndustryType = type;

            return this;
        }

        public CompanyBuilder WithLeadId(Guid leadId)
        {
            _company.LeadId = leadId;

            return this;
        }

        public CompanyBuilder WithCreateUserId(Guid createUserId)
        {
            _company.CreateUserId = createUserId;

            return this;
        }

        public CompanyBuilder WithResponsibleUserId(Guid responsibleUserId)
        {
            _company.ResponsibleUserId = responsibleUserId;

            return this;
        }

        public CompanyBuilder WithFullName(string fullName)
        {
            _company.FullName = fullName;

            return this;
        }

        public CompanyBuilder WithShortName(string shortName)
        {
            _company.ShortName = shortName;

            return this;
        }

        public CompanyBuilder WithPhone(string phone)
        {
            _company.Phone = phone;

            return this;
        }

        public CompanyBuilder WithEmail(string email)
        {
            _company.Email = email;

            return this;
        }

        public CompanyBuilder WithTaxNumber(string taxNumber)
        {
            _company.TaxNumber = taxNumber;

            return this;
        }

        public CompanyBuilder WithRegistrationNumber(string registrationNumber)
        {
            _company.RegistrationNumber = registrationNumber;

            return this;
        }

        public CompanyBuilder WithRegistrationDate(DateTime registrationDate)
        {
            _company.RegistrationDate = registrationDate;

            return this;
        }

        public CompanyBuilder WithEmployeesCount(int employeesCount)
        {
            _company.EmployeesCount = employeesCount;

            return this;
        }

        public CompanyBuilder WithYearlyTurnover(int yearlyTurnover)
        {
            _company.YearlyTurnover = yearlyTurnover;

            return this;
        }

        public CompanyBuilder WithPostcode(string postcode)
        {
            _company.JuridicalPostcode = postcode;

            return this;
        }

        public CompanyBuilder WithJuridicalCountry(string juridicalCountry)
        {
            _company.JuridicalCountry = juridicalCountry;

            return this;
        }

        public CompanyBuilder WithJuridicalRegion(string juridicalRegion)
        {
            _company.JuridicalRegion = juridicalRegion;

            return this;
        }

        public CompanyBuilder WithJuridicalProvince(string juridicalProvince)
        {
            _company.JuridicalProvince = juridicalProvince;

            return this;
        }

        public CompanyBuilder WithJuridicalCity(string juridicalCity)
        {
            _company.JuridicalCity = juridicalCity;

            return this;
        }

        public CompanyBuilder WithJuridicalStreet(string juridicalStreet)
        {
            _company.JuridicalStreet = juridicalStreet;

            return this;
        }

        public CompanyBuilder WithJuridicalHouse(string juridicalHouse)
        {
            _company.JuridicalHouse = juridicalHouse;

            return this;
        }

        public CompanyBuilder WithJuridicalApartment(string juridicalApartment)
        {
            _company.JuridicalApartment = juridicalApartment;

            return this;
        }

        public CompanyBuilder WithLegalCountry(string legalCountry)
        {
            _company.LegalCountry = legalCountry;

            return this;
        }

        public CompanyBuilder WithLegalRegion(string legalRegion)
        {
            _company.LegalRegion = legalRegion;

            return this;
        }

        public CompanyBuilder WithLegalProvince(string legalProvince)
        {
            _company.LegalProvince = legalProvince;

            return this;
        }

        public CompanyBuilder WithLegalCity(string legalCity)
        {
            _company.LegalCity = legalCity;

            return this;
        }

        public CompanyBuilder WithLegalStreet(string legalStreet)
        {
            _company.LegalStreet = legalStreet;

            return this;
        }

        public CompanyBuilder WithLegalHouse(string legalHouse)
        {
            _company.LegalHouse = legalHouse;

            return this;
        }

        public CompanyBuilder WithLegalApartment(string legalApartment)
        {
            _company.LegalApartment = legalApartment;

            return this;
        }

        public CompanyBuilder AsDeleted()
        {
            _company.IsDeleted = true;

            return this;
        }

        public CompanyBuilder WithBankAccount(
            string number,
            string bankNumber,
            string bankName,
            string bankCorrespondentNumber)
        {
            if (_company.BankAccounts == null)
            {
                _company.BankAccounts = new List<CompanyBankAccount>();
            }

            _company.BankAccounts.Add(new CompanyBankAccount
            {
                Number = number,
                BankNumber = bankNumber,
                BankName = bankName,
                BankCorrespondentNumber = bankCorrespondentNumber
            });

            return this;
        }

        public CompanyBuilder WithAttributeLink(Guid attributeId, string value)
        {
            if (_company.AttributeLinks == null)
            {
                _company.AttributeLinks = new List<CompanyAttributeLink>();
            }

            _company.AttributeLinks.Add(new CompanyAttributeLink
            {
                CompanyAttributeId = attributeId,
                Value = value
            });

            return this;
        }

        public async Task<Company> BuildAsync()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            if (_company.LeadId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_company.LeadId));
            }

            var id = await _companiesClient.CreateAsync(accessToken, _company);

            return await _companiesClient.GetAsync(accessToken, id);
        }
    }
}
