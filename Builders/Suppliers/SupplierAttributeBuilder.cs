using System;
using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Suppliers.Clients;
using Crm.v1.Clients.Suppliers.Models;

namespace Crm.Tests.All.Builders.Suppliers
{
    public class SupplierAttributeBuilder : ISupplierAttributeBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ISupplierAttributesClient _customerAttributesClient;
        private readonly SupplierAttribute _attribute;

        public SupplierAttributeBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ISupplierAttributesClient customerAttributesClient)
        {
            _customerAttributesClient = customerAttributesClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _attribute = new SupplierAttribute
            {
                Id = Guid.NewGuid(),
                Type = AttributeType.Text,
                Key = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public SupplierAttributeBuilder WithType(AttributeType type)
        {
            _attribute.Type = type;

            return this;
        }

        public SupplierAttributeBuilder WithKey(string key)
        {
            _attribute.Key = key;

            return this;
        }

        public SupplierAttributeBuilder AsDeleted()
        {
            _attribute.IsDeleted = true;

            return this;
        }

        public async Task<SupplierAttribute> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var id = await _customerAttributesClient.CreateAsync(_attribute, headers);

            return await _customerAttributesClient.GetAsync(id, headers);
        }
    }
}
