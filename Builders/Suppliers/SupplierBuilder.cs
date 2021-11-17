using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Suppliers.Clients;
using Crm.v1.Clients.Suppliers.Models;

namespace Crm.Tests.All.Builders.Suppliers
{
    public class SupplierBuilder : ISupplierBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ISuppliersClient _customersClient;
        private readonly Supplier _customer;

        public SupplierBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ISuppliersClient customersClient)
        {
            _customersClient = customersClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _customer = new Supplier
            {
                Id = Guid.NewGuid(),
                Name = "Test".WithGuid(),
                Phone = "9999999999",
                Email = "test@test",
                IsDeleted = false
            };
        }

        public SupplierBuilder WithCreateUserId(Guid createUserId)
        {
            _customer.CreateUserId = createUserId;

            return this;
        }

        public SupplierBuilder WithName(string name)
        {
            _customer.Name = name;

            return this;
        }

        public SupplierBuilder WithPhone(string phone)
        {
            _customer.Phone = phone;

            return this;
        }

        public SupplierBuilder WithEmail(string email)
        {
            _customer.Email = email;

            return this;
        }

        public SupplierBuilder AsDeleted()
        {
            _customer.IsDeleted = true;

            return this;
        }

        public SupplierBuilder WithAttributeLink(Guid attributeId, string value)
        {
            _customer.AttributeLinks ??= new List<SupplierAttributeLink>();
            _customer.AttributeLinks.Add(new SupplierAttributeLink
            {
                SupplierAttributeId = attributeId,
                Value = value
            });

            return this;
        }

        public async Task<Supplier> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var id = await _customersClient.CreateAsync(_customer, headers);

            return await _customersClient.GetAsync(id, headers);
        }
    }
}
