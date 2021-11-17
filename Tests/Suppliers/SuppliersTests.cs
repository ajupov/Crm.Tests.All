using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Suppliers.Clients;
using Crm.v1.Clients.Suppliers.Models;
using Xunit;

namespace Crm.Tests.All.Tests.Suppliers
{
    public class SuppliersTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly ISuppliersClient _suppliersClient;

        public SuppliersTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            ISuppliersClient suppliersClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _suppliersClient = suppliersClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var supplierId = (await _create.Supplier.BuildAsync()).Id;

            var supplier = await _suppliersClient.GetAsync(supplierId, headers);

            Assert.NotNull(supplier);
            Assert.Equal(supplierId, supplier.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var supplierIds = (
                    await Task.WhenAll(
                        _create.Supplier
                            .BuildAsync(),
                        _create.Supplier
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var suppliers = await _suppliersClient.GetListAsync(supplierIds, headers);

            Assert.NotEmpty(suppliers);
            Assert.Equal(supplierIds.Count, suppliers.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = await _create.SupplierAttribute.BuildAsync();
            var value = "Test".WithGuid();

            await Task.WhenAll(
                _create.Supplier
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync(),
                _create.Supplier
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync());
            var filterAttributes = new Dictionary<Guid, string> { { attribute.Id, value } };

            var request = new SupplierGetPagedListRequest
            {
                Attributes = filterAttributes
            };

            var response = await _suppliersClient.GetPagedListAsync(request, headers);

            var results = response.Suppliers
                .Skip(1)
                .Zip(response.Suppliers, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Suppliers);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = await _create.SupplierAttribute.BuildAsync();

            var supplier = new Supplier
            {
                Id = Guid.NewGuid(),
                Name = "Test".WithGuid(),
                Phone = "9999999999",
                Email = "test@test",
                IsDeleted = true,
                AttributeLinks = new List<SupplierAttributeLink>
                {
                    new ()
                    {
                        SupplierAttributeId = attribute.Id,
                        Value = "Test".WithGuid()
                    }
                }
            };

            var createdSupplierId = await _suppliersClient.CreateAsync(supplier, headers);

            var createdSupplier = await _suppliersClient.GetAsync(createdSupplierId, headers);

            Assert.NotNull(createdSupplier);
            Assert.Equal(createdSupplierId, createdSupplier.Id);
            Assert.True(!createdSupplier.CreateUserId.IsEmpty());
            Assert.Equal(supplier.Name, createdSupplier.Name);
            Assert.Equal(supplier.Phone, createdSupplier.Phone);
            Assert.Equal(supplier.Email, createdSupplier.Email);
            Assert.Equal(supplier.IsDeleted, createdSupplier.IsDeleted);
            Assert.True(createdSupplier.CreateDateTime.IsMoreThanMinValue());
            Assert.NotEmpty(createdSupplier.AttributeLinks);
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = await _create.SupplierAttribute.BuildAsync();
            var supplier = await _create.Supplier
                .BuildAsync();

            supplier.Name = "Test".WithGuid();
            supplier.Phone = "9999999999";
            supplier.Email = "test@test";
            supplier.IsDeleted = true;
            supplier.AttributeLinks.Add(new SupplierAttributeLink
                { SupplierAttributeId = attribute.Id, Value = "Test".WithGuid() });
            await _suppliersClient.UpdateAsync(supplier, headers);

            var updatedSupplier = await _suppliersClient.GetAsync(supplier.Id, headers);

            Assert.Equal(supplier.AccountId, updatedSupplier.AccountId);
            Assert.Equal(supplier.CreateUserId, updatedSupplier.CreateUserId);
            Assert.Equal(supplier.Name, updatedSupplier.Name);
            Assert.Equal(supplier.Phone, updatedSupplier.Phone);
            Assert.Equal(supplier.Email, updatedSupplier.Email);
            Assert.Equal(supplier.IsDeleted, updatedSupplier.IsDeleted);
            Assert.Equal(supplier.AttributeLinks.Single().Value, updatedSupplier.AttributeLinks.Single().Value);
            Assert.Equal(
                supplier.AttributeLinks.Single().SupplierAttributeId,
                updatedSupplier.AttributeLinks.Single().SupplierAttributeId);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var supplierIds = (
                    await Task.WhenAll(
                        _create.Supplier
                            .BuildAsync(),
                        _create.Supplier
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _suppliersClient.DeleteAsync(supplierIds, headers);

            var suppliers = await _suppliersClient.GetListAsync(supplierIds, headers);

            Assert.All(suppliers, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var supplierIds = (
                    await Task.WhenAll(
                        _create.Supplier
                            .BuildAsync(),
                        _create.Supplier
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _suppliersClient.RestoreAsync(supplierIds, headers);

            var suppliers = await _suppliersClient.GetListAsync(supplierIds, headers);

            Assert.All(suppliers, x => Assert.False(x.IsDeleted));
        }
    }
}
