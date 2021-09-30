using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Customers.Clients;
using Crm.v1.Clients.Customers.Models;

namespace Crm.Tests.All.Builders.Customers
{
    public class CustomerBuilder : ICustomerBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ICustomersClient _customersClient;
        private readonly Customer _customer;

        public CustomerBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ICustomersClient customersClient)
        {
            _customersClient = customersClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _customer = new Customer
            {
                Surname = "Test".WithGuid(),
                Name = "Test".WithGuid(),
                Patronymic = "Test".WithGuid(),
                Phone = "9999999999",
                Email = "test@test",
                BirthDate = new DateTime(1990, 1, 1),
                IsDeleted = false
            };
        }

        public CustomerBuilder WithSourceId(Guid sourceId)
        {
            _customer.SourceId = sourceId;

            return this;
        }

        public CustomerBuilder WithCreateUserId(Guid createUserId)
        {
            _customer.CreateUserId = createUserId;

            return this;
        }

        public CustomerBuilder WithResponsibleUserId(Guid responsibleUserId)
        {
            _customer.ResponsibleUserId = responsibleUserId;

            return this;
        }

        public CustomerBuilder WithSurname(string surname)
        {
            _customer.Surname = surname;

            return this;
        }

        public CustomerBuilder WithName(string name)
        {
            _customer.Name = name;

            return this;
        }

        public CustomerBuilder WithPatronymic(string patronymic)
        {
            _customer.Patronymic = patronymic;

            return this;
        }

        public CustomerBuilder WithPhone(string phone)
        {
            _customer.Phone = phone;

            return this;
        }

        public CustomerBuilder WithEmail(string email)
        {
            _customer.Email = email;

            return this;
        }

        public CustomerBuilder WithBirthDate(DateTime birthDate)
        {
            _customer.BirthDate = birthDate;

            return this;
        }

        public CustomerBuilder AsDeleted()
        {
            _customer.IsDeleted = true;

            return this;
        }

        public CustomerBuilder WithAttributeLink(Guid attributeId, string value)
        {
            _customer.AttributeLinks ??= new List<CustomerAttributeLink>();
            _customer.AttributeLinks.Add(new CustomerAttributeLink
            {
                CustomerAttributeId = attributeId,
                Value = value
            });

            return this;
        }

        public async Task<Customer> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            if (_customer.SourceId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_customer.SourceId));
            }

            var id = await _customersClient.CreateAsync(_customer, headers);

            return await _customersClient.GetAsync(id, headers);
        }
    }
}
