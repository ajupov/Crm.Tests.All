using System;
using System.Threading.Tasks;
using Crm.v1.Clients.Customers.Models;

namespace Crm.Tests.All.Builders.Customers
{
    public interface ICustomerBuilder
    {
        CustomerBuilder WithSourceId(Guid sourceId);

        CustomerBuilder WithCreateUserId(Guid createUserId);

        CustomerBuilder WithResponsibleUserId(Guid responsibleUserId);

        CustomerBuilder WithSurname(string surname);

        CustomerBuilder WithName(string name);

        CustomerBuilder WithPatronymic(string patronymic);

        CustomerBuilder WithPhone(string phone);

        CustomerBuilder WithEmail(string email);

        CustomerBuilder WithBirthDate(DateTime birthDate);

        CustomerBuilder AsDeleted();

        CustomerBuilder WithAttributeLink(Guid attributeId, string value);

        Task<Customer> BuildAsync();
    }
}
