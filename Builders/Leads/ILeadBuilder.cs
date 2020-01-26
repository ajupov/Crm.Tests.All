using System;
using System.Threading.Tasks;
using Crm.v1.Clients.Leads.Models;

namespace Crm.Tests.All.Builders.Leads
{
    public interface ILeadBuilder
    {
        LeadBuilder WithSourceId(Guid sourceId);

        LeadBuilder WithCreateUserId(Guid createUserId);

        LeadBuilder WithResponsibleUserId(Guid responsibleUserId);

        LeadBuilder WithSurname(string surname);

        LeadBuilder WithName(string name);

        LeadBuilder WithPatronymic(string patronymic);

        LeadBuilder WithPhone(string phone);

        LeadBuilder WithEmail(string email);

        LeadBuilder WithCompanyName(string companyName);

        LeadBuilder WithPost(string post);

        LeadBuilder WithPostcode(string postcode);

        LeadBuilder WithCountry(string country);

        LeadBuilder WithRegion(string region);

        LeadBuilder WithProvince(string province);

        LeadBuilder WithCity(string city);

        LeadBuilder WithStreet(string street);

        LeadBuilder WithHouse(string house);

        LeadBuilder WithApartment(string apartment);

        LeadBuilder WithOpportunitySum(decimal opportunitySum);

        LeadBuilder AsDeleted();

        LeadBuilder WithAttributeLink(Guid attributeId, string value);

        Task<Lead> BuildAsync();
    }
}