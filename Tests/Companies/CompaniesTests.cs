using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Companies.Clients;
using Crm.v1.Clients.Companies.Models;
using Crm.v1.Clients.Companies.RequestParameters;
using Xunit;

namespace Crm.Tests.All.Tests.Companies
{
    public class CompanyTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly ICompaniesClient _companiesClient;

        public CompanyTests(IAccessTokenGetter accessTokenGetter, ICreate create, ICompaniesClient companiesClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _companiesClient = companiesClient;
        }

        [Fact]
        public async Task WhenGetTypes_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var types = await _companiesClient.GetTypesAsync(accessToken);

            Assert.NotEmpty(types);
        }

        [Fact]
        public async Task WhenGetIndustryTypes_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var types = await _companiesClient.GetIndustryTypesAsync(accessToken);

            Assert.NotEmpty(types);
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var leadSource = await _create.LeadSource.BuildAsync();
            var lead = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var companyId = (
                    await _create.Company
                        .WithLeadId(lead.Id)
                        .BuildAsync())
                .Id;

            var company = await _companiesClient.GetAsync(accessToken, companyId);

            Assert.NotNull(company);
            Assert.Equal(companyId, company.Id);
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
            var companyIds = (
                    await Task.WhenAll(
                        _create.Company
                            .WithLeadId(lead1.Id)
                            .WithTaxNumber("999999999999".WithGuid())
                            .BuildAsync(),
                        _create.Company
                            .WithLeadId(lead2.Id)
                            .WithTaxNumber("999999999999".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var companies = await _companiesClient.GetListAsync(accessToken, companyIds);

            Assert.NotEmpty(companies);
            Assert.Equal(companyIds.Count, companies.Count);
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
            var attribute = await _create.CompanyAttribute.BuildAsync();

            var value = "Test".WithGuid();
            await Task.WhenAll(
                _create.Company
                    .WithLeadId(lead1.Id)
                    .WithTaxNumber("999999999999".WithGuid())
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync(),
                _create.Company
                    .WithLeadId(lead2.Id)
                    .WithTaxNumber("999999999999".WithGuid())
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync());
            var filterAttributes = new Dictionary<Guid, string> {{attribute.Id, value}};

            var request = new CompanyGetPagedListRequestParameter
            {
                AllAttributes = false,
                Attributes = filterAttributes,
            };

            var companies = await _companiesClient.GetPagedListAsync(accessToken, request);

            var results = companies
                .Skip(1)
                .Zip(companies, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(companies);
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
            var attribute = await _create.CompanyAttribute.BuildAsync();

            var company = new Company
            {
                LeadId = lead.Id,
                Type = CompanyType.SelfEmployed,
                IndustryType = CompanyIndustryType.Transport,
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
                IsDeleted = true,
                AttributeLinks = new List<CompanyAttributeLink>
                {
                    new CompanyAttributeLink
                    {
                        CompanyAttributeId = attribute.Id,
                        Value = "Test".WithGuid()
                    }
                },
                BankAccounts = new List<CompanyBankAccount>
                {
                    new CompanyBankAccount
                    {
                        Number = "9999999999999999999999999",
                        BankNumber = "9999999999",
                        BankCorrespondentNumber = "9999999999999999999999999",
                        BankName = "Test".WithGuid()
                    }
                }
            };

            var createdCompanyId = await _companiesClient.CreateAsync(accessToken, company);

            var createdCompany = await _companiesClient.GetAsync(accessToken, createdCompanyId);

            Assert.NotNull(createdCompany);
            Assert.Equal(createdCompanyId, createdCompany.Id);
            Assert.Equal(company.Type, createdCompany.Type);
            Assert.Equal(company.IndustryType, createdCompany.IndustryType);
            Assert.Equal(company.LeadId, createdCompany.LeadId);
            Assert.True(!createdCompany.CreateUserId.IsEmpty());
            Assert.Equal(company.ResponsibleUserId, createdCompany.ResponsibleUserId);
            Assert.Equal(company.FullName, createdCompany.FullName);
            Assert.Equal(company.ShortName, createdCompany.ShortName);
            Assert.Equal(company.Phone, createdCompany.Phone);
            Assert.Equal(company.Email, createdCompany.Email);
            Assert.Equal(company.TaxNumber, createdCompany.TaxNumber);
            Assert.Equal(company.RegistrationNumber, createdCompany.RegistrationNumber);
            Assert.Equal(company.RegistrationDate, createdCompany.RegistrationDate);
            Assert.Equal(company.EmployeesCount, createdCompany.EmployeesCount);
            Assert.Equal(company.YearlyTurnover, createdCompany.YearlyTurnover);
            Assert.Equal(company.JuridicalPostcode, createdCompany.JuridicalPostcode);
            Assert.Equal(company.JuridicalCountry, createdCompany.JuridicalCountry);
            Assert.Equal(company.JuridicalRegion, createdCompany.JuridicalRegion);
            Assert.Equal(company.JuridicalProvince, createdCompany.JuridicalProvince);
            Assert.Equal(company.JuridicalCity, createdCompany.JuridicalCity);
            Assert.Equal(company.JuridicalStreet, createdCompany.JuridicalStreet);
            Assert.Equal(company.JuridicalHouse, createdCompany.JuridicalHouse);
            Assert.Equal(company.JuridicalApartment, createdCompany.JuridicalApartment);
            Assert.Equal(company.LegalPostcode, createdCompany.LegalPostcode);
            Assert.Equal(company.LegalCountry, createdCompany.LegalCountry);
            Assert.Equal(company.LegalRegion, createdCompany.LegalRegion);
            Assert.Equal(company.LegalProvince, createdCompany.LegalProvince);
            Assert.Equal(company.LegalCity, createdCompany.LegalCity);
            Assert.Equal(company.LegalStreet, createdCompany.LegalStreet);
            Assert.Equal(company.LegalHouse, createdCompany.LegalHouse);
            Assert.Equal(company.LegalApartment, createdCompany.LegalApartment);
            Assert.Equal(company.IsDeleted, createdCompany.IsDeleted);
            Assert.True(createdCompany.CreateDateTime.IsMoreThanMinValue());
            Assert.NotEmpty(createdCompany.AttributeLinks);
            Assert.NotEmpty(createdCompany.BankAccounts);
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var leadSource = await _create.LeadSource.BuildAsync();
            var lead = await _create.Lead
                .WithSourceId(leadSource.Id)
                .BuildAsync();
            var attribute = await _create.CompanyAttribute.BuildAsync();
            var company = await _create.Company
                .WithLeadId(lead.Id)
                .BuildAsync();

            company.Type = CompanyType.Commercial;
            company.IndustryType = CompanyIndustryType.Computer;
            company.FullName = "Test".WithGuid();
            company.ShortName = "Test".WithGuid();
            company.Phone = "9999999999";
            company.Email = "test@test";
            company.TaxNumber = "999999999999".WithGuid();
            company.RegistrationNumber = "999999999999999";
            company.RegistrationDate = new DateTime(2000, 1, 1);
            company.EmployeesCount = 1;
            company.YearlyTurnover = 1000000;
            company.JuridicalPostcode = "000000";
            company.JuridicalCountry = "Test".WithGuid();
            company.JuridicalRegion = "Test".WithGuid();
            company.JuridicalProvince = "Test".WithGuid();
            company.JuridicalCity = "Test".WithGuid();
            company.JuridicalStreet = "Test".WithGuid();
            company.JuridicalHouse = "1";
            company.JuridicalApartment = "1";
            company.LegalPostcode = "000000";
            company.LegalCountry = "Test".WithGuid();
            company.LegalRegion = "Test".WithGuid();
            company.LegalProvince = "Test".WithGuid();
            company.LegalCity = "Test".WithGuid();
            company.LegalStreet = "Test".WithGuid();
            company.LegalHouse = "1";
            company.LegalApartment = "1";
            company.IsDeleted = true;
            company.AttributeLinks.Add(new CompanyAttributeLink
                {CompanyAttributeId = attribute.Id, Value = "Test".WithGuid()});
            company.BankAccounts.Add(new CompanyBankAccount
            {
                Number = "9999999999999999999999999",
                BankNumber = "9999999999",
                BankCorrespondentNumber = "9999999999999999999999999",
                BankName = "Test".WithGuid()
            });
            await _companiesClient.UpdateAsync(accessToken, company);

            var updatedCompany = await _companiesClient.GetAsync(accessToken, company.Id);

            Assert.Equal(company.AccountId, updatedCompany.AccountId);
            Assert.Equal(company.Type, updatedCompany.Type);
            Assert.Equal(company.IndustryType, updatedCompany.IndustryType);
            Assert.Equal(company.LeadId, updatedCompany.LeadId);
            Assert.Equal(company.CreateUserId, updatedCompany.CreateUserId);
            Assert.Equal(company.ResponsibleUserId, updatedCompany.ResponsibleUserId);
            Assert.Equal(company.FullName, updatedCompany.FullName);
            Assert.Equal(company.ShortName, updatedCompany.ShortName);
            Assert.Equal(company.AccountId, updatedCompany.AccountId);
            Assert.Equal(company.AccountId, updatedCompany.AccountId);
            Assert.Equal(company.AccountId, updatedCompany.AccountId);
            Assert.Equal(company.Phone, updatedCompany.Phone);
            Assert.Equal(company.Email, updatedCompany.Email);
            Assert.Equal(company.TaxNumber, updatedCompany.TaxNumber);
            Assert.Equal(company.RegistrationNumber, updatedCompany.RegistrationNumber);
            Assert.Equal(company.RegistrationDate, updatedCompany.RegistrationDate);
            Assert.Equal(company.EmployeesCount, updatedCompany.EmployeesCount);
            Assert.Equal(company.YearlyTurnover, updatedCompany.YearlyTurnover);
            Assert.Equal(company.JuridicalPostcode, updatedCompany.JuridicalPostcode);
            Assert.Equal(company.JuridicalCountry, updatedCompany.JuridicalCountry);
            Assert.Equal(company.JuridicalRegion, updatedCompany.JuridicalRegion);
            Assert.Equal(company.JuridicalProvince, updatedCompany.JuridicalProvince);
            Assert.Equal(company.JuridicalCity, updatedCompany.JuridicalCity);
            Assert.Equal(company.JuridicalStreet, updatedCompany.JuridicalStreet);
            Assert.Equal(company.JuridicalHouse, updatedCompany.JuridicalHouse);
            Assert.Equal(company.JuridicalApartment, updatedCompany.JuridicalApartment);
            Assert.Equal(company.LegalPostcode, updatedCompany.LegalPostcode);
            Assert.Equal(company.LegalCountry, updatedCompany.LegalCountry);
            Assert.Equal(company.LegalRegion, updatedCompany.LegalRegion);
            Assert.Equal(company.LegalProvince, updatedCompany.LegalProvince);
            Assert.Equal(company.LegalCity, updatedCompany.LegalCity);
            Assert.Equal(company.LegalStreet, updatedCompany.LegalStreet);
            Assert.Equal(company.LegalHouse, updatedCompany.LegalHouse);
            Assert.Equal(company.LegalApartment, updatedCompany.LegalApartment);
            Assert.Equal(company.IsDeleted, updatedCompany.IsDeleted);
            Assert.Equal(company.AttributeLinks.Single().CompanyAttributeId,
                updatedCompany.AttributeLinks.Single().CompanyAttributeId);
            Assert.Equal(company.AttributeLinks.Single().Value, updatedCompany.AttributeLinks.Single().Value);
            Assert.Equal(company.BankAccounts.Single().Number, updatedCompany.BankAccounts.Single().Number);
            Assert.Equal(company.BankAccounts.Single().BankNumber, updatedCompany.BankAccounts.Single().BankNumber);
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
            var companyIds = (
                    await Task.WhenAll(
                        _create.Company
                            .WithLeadId(lead1.Id)
                            .WithTaxNumber("999999999999".WithGuid())
                            .BuildAsync(),
                        _create.Company
                            .WithLeadId(lead2.Id)
                            .WithTaxNumber("999999999999".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id).ToList();

            await _companiesClient.DeleteAsync(accessToken, companyIds);

            var companies = await _companiesClient.GetListAsync(accessToken, companyIds);

            Assert.All(companies, x => Assert.True(x.IsDeleted));
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
            var companyIds = (
                    await Task.WhenAll(
                        _create.Company
                            .WithLeadId(lead1.Id)
                            .WithTaxNumber("999999999999".WithGuid())
                            .BuildAsync(),
                        _create.Company
                            .WithLeadId(lead2.Id)
                            .WithTaxNumber("999999999999".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _companiesClient.RestoreAsync(accessToken, companyIds);

            var companies = await _companiesClient.GetListAsync(accessToken, companyIds);

            Assert.All(companies, x => Assert.False(x.IsDeleted));
        }
    }
}