using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Customers.Clients;
using Crm.v1.Clients.Customers.Models;

namespace Crm.Tests.All.Builders.Customers
{
    public class CustomerAttributeBuilder : ICustomerAttributeBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ICustomerAttributesClient _customerAttributesClient;
        private readonly CustomerAttribute _attribute;

        public CustomerAttributeBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ICustomerAttributesClient customerAttributesClient)
        {
            _customerAttributesClient = customerAttributesClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _attribute = new CustomerAttribute
            {
                Type = AttributeType.Text,
                Key = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public CustomerAttributeBuilder WithType(AttributeType type)
        {
            _attribute.Type = type;

            return this;
        }

        public CustomerAttributeBuilder WithKey(string key)
        {
            _attribute.Key = key;

            return this;
        }

        public CustomerAttributeBuilder AsDeleted()
        {
            _attribute.IsDeleted = true;

            return this;
        }

        public async Task<CustomerAttribute> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var id = await _customerAttributesClient.CreateAsync(_attribute, headers);

            return await _customerAttributesClient.GetAsync(id, headers);
        }
    }
}
