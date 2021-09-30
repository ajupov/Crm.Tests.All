using System.Threading.Tasks;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Customers.Clients;
using Crm.v1.Clients.Customers.Models;

namespace Crm.Tests.All.Builders.Customers
{
    public class CustomerSourceBuilder : ICustomerSourceBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ICustomerSourcesClient _customerSourcesClient;
        private readonly CustomerSource _source;

        public CustomerSourceBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ICustomerSourcesClient customerSourcesClient)
        {
            _customerSourcesClient = customerSourcesClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _source = new CustomerSource
            {
                Name = "Test".WithGuid(),
                IsDeleted = false
            };
        }

        public CustomerSourceBuilder WithName(string name)
        {
            _source.Name = name;

            return this;
        }

        public CustomerSourceBuilder AsDeleted()
        {
            _source.IsDeleted = true;

            return this;
        }

        public async Task<CustomerSource> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var id = await _customerSourcesClient.CreateAsync(_source, headers);

            return await _customerSourcesClient.GetAsync(id, headers);
        }
    }
}
