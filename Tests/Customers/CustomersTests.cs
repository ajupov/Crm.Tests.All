using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Customers.Clients;
using Crm.v1.Clients.Customers.Models;
using Crm.v1.Clients.Customers.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Customers
{
    public class CustomersTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ICustomersClient _customersClient;

        public CustomersTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ICustomersClient customersClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _customersClient = customersClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var source = await _create.CustomerSource.BuildAsync();
            var customerId = (await _create.Customer.WithSourceId(source.Id).BuildAsync()).Id;

            var customer = await _customersClient.GetAsync(customerId, headers);

            Assert.NotNull(customer);
            Assert.Equal(customerId, customer.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var source = await _create.CustomerSource.BuildAsync();
            var customerIds = (
                    await Task.WhenAll(
                        _create.Customer
                            .WithSourceId(source.Id)
                            .BuildAsync(),
                        _create.Customer
                            .WithSourceId(source.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var customers = await _customersClient.GetListAsync(customerIds, headers);

            Assert.NotEmpty(customers);
            Assert.Equal(customerIds.Count, customers.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = await _create.CustomerAttribute.BuildAsync();
            var value = "Test".WithGuid();
            var source = await _create.CustomerSource
                .WithName(value)
                .BuildAsync();
            await Task.WhenAll(
                _create.Customer
                    .WithSourceId(source.Id)
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync(),
                _create.Customer
                    .WithSourceId(source.Id)
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync());
            var filterAttributes = new Dictionary<Guid, string> { { attribute.Id, value } };
            var filterSourceIds = new List<Guid> { source.Id };

            var request = new CustomerGetPagedListRequest
            {
                Attributes = filterAttributes, SourceIds = filterSourceIds
            };

            var response = await _customersClient.GetPagedListAsync(request, headers);

            var results = response.Customers
                .Skip(1)
                .Zip(response.Customers, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Customers);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = await _create.CustomerAttribute.BuildAsync();
            var source = await _create.CustomerSource.BuildAsync();

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                SourceId = source.Id,
                Surname = "Test".WithGuid(),
                Name = "Test".WithGuid(),
                Patronymic = "Test".WithGuid(),
                Phone = "9999999999",
                Email = "test@test",
                BirthDate = new DateTime(1990, 1, 1),
                IsDeleted = true,
                AttributeLinks = new List<CustomerAttributeLink>
                {
                    new ()
                    {
                        CustomerAttributeId = attribute.Id,
                        Value = "Test".WithGuid()
                    }
                }
            };

            var createdCustomerId = await _customersClient.CreateAsync(customer, headers);

            var createdCustomer = await _customersClient.GetAsync(createdCustomerId, headers);

            Assert.NotNull(createdCustomer);
            Assert.Equal(createdCustomerId, createdCustomer.Id);
            Assert.Equal(customer.SourceId, createdCustomer.SourceId);
            Assert.True(!createdCustomer.CreateUserId.IsEmpty());
            Assert.Equal(customer.ResponsibleUserId, createdCustomer.ResponsibleUserId);
            Assert.Equal(customer.Surname, createdCustomer.Surname);
            Assert.Equal(customer.Name, createdCustomer.Name);
            Assert.Equal(customer.Patronymic, createdCustomer.Patronymic);
            Assert.Equal(customer.Phone, createdCustomer.Phone);
            Assert.Equal(customer.Email, createdCustomer.Email);
            Assert.Equal(customer.IsDeleted, createdCustomer.IsDeleted);
            Assert.True(createdCustomer.CreateDateTime.IsMoreThanMinValue());
            Assert.NotEmpty(createdCustomer.AttributeLinks);
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var source = await _create.CustomerSource.BuildAsync();
            var attribute = await _create.CustomerAttribute.BuildAsync();
            var customer = await _create.Customer
                .WithSourceId(source.Id)
                .BuildAsync();

            customer.SourceId = source.Id;
            customer.Surname = "Test".WithGuid();
            customer.Name = "Test".WithGuid();
            customer.Patronymic = "Test".WithGuid();
            customer.Phone = "9999999999";
            customer.Email = "test@test";
            customer.IsDeleted = true;
            customer.AttributeLinks.Add(new CustomerAttributeLink
                { CustomerAttributeId = attribute.Id, Value = "Test".WithGuid() });
            await _customersClient.UpdateAsync(customer, headers);

            var updatedCustomer = await _customersClient.GetAsync(customer.Id, headers);

            Assert.Equal(customer.AccountId, updatedCustomer.AccountId);
            Assert.Equal(customer.SourceId, updatedCustomer.SourceId);
            Assert.Equal(customer.CreateUserId, updatedCustomer.CreateUserId);
            Assert.Equal(customer.ResponsibleUserId, updatedCustomer.ResponsibleUserId);
            Assert.Equal(customer.Surname, updatedCustomer.Surname);
            Assert.Equal(customer.Name, updatedCustomer.Name);
            Assert.Equal(customer.Patronymic, updatedCustomer.Patronymic);
            Assert.Equal(customer.Phone, updatedCustomer.Phone);
            Assert.Equal(customer.Email, updatedCustomer.Email);
            Assert.Equal(customer.IsDeleted, updatedCustomer.IsDeleted);
            Assert.Equal(customer.AttributeLinks.Single().Value, updatedCustomer.AttributeLinks.Single().Value);
            Assert.Equal(
                customer.AttributeLinks.Single().CustomerAttributeId,
                updatedCustomer.AttributeLinks.Single().CustomerAttributeId);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var source = await _create.CustomerSource.BuildAsync();
            var customerIds = (
                    await Task.WhenAll(
                        _create.Customer
                            .WithSourceId(source.Id)
                            .BuildAsync(),
                        _create.Customer
                            .WithSourceId(source.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _customersClient.DeleteAsync(customerIds, headers);

            var customers = await _customersClient.GetListAsync(customerIds, headers);

            Assert.All(customers, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var source = await _create.CustomerSource.BuildAsync();
            var customerIds = (
                    await Task.WhenAll(
                        _create.Customer
                            .WithSourceId(source.Id)
                            .BuildAsync(),
                        _create.Customer
                            .WithSourceId(source.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _customersClient.RestoreAsync(customerIds, headers);

            var customers = await _customersClient.GetListAsync(customerIds, headers);

            Assert.All(customers, x => Assert.False(x.IsDeleted));
        }
    }
}
