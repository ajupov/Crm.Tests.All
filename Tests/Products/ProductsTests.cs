using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;
using Xunit;

namespace Crm.Tests.All.Tests.Products
{
    public class ProductsTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IProductsClient _productsClient;

        public ProductsTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IProductsClient productsClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _productsClient = productsClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = await _create.ProductStatus.BuildAsync();
            var productId = (
                    await _create.Product
                        .WithStatusId(status.Id)
                        .BuildAsync())
                .Id;

            var product = await _productsClient.GetAsync(productId, headers);

            Assert.NotNull(product);
            Assert.Equal(productId, product.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = await _create.ProductStatus.BuildAsync();
            var productIds = (
                    await Task.WhenAll(
                        _create.Product
                            .WithStatusId(status.Id)
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.Product
                            .WithStatusId(status.Id)
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var products = await _productsClient.GetListAsync(productIds, headers);

            Assert.NotEmpty(products);
            Assert.Equal(productIds.Count, products.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = await _create.ProductAttribute.BuildAsync();
            var category = await _create.ProductCategory.BuildAsync();
            var status = await _create.ProductStatus.BuildAsync();
            var value = "Test".WithGuid();
            await Task.WhenAll(
                _create.Product
                    .WithStatusId(status.Id)
                    .WithName("Test".WithGuid())
                    .WithAttributeLink(attribute.Id, value)
                    .WithCategoryLink(category.Id)
                    .BuildAsync(),
                _create.Product
                    .WithStatusId(status.Id)
                    .WithName("Test".WithGuid())
                    .WithAttributeLink(attribute.Id, value)
                    .WithCategoryLink(category.Id)
                    .BuildAsync());

            var filterAttributes = new Dictionary<Guid, string> { { attribute.Id, value } };
            var filterCategoryIds = new List<Guid> { category.Id };

            var request = new ProductGetPagedListRequest
            {
                Attributes = filterAttributes,
                CategoryIds = filterCategoryIds
            };

            var response = await _productsClient.GetPagedListAsync(request, headers);

            var results = response.Products
                .Skip(1)
                .Zip(response.Products, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Products);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var attribute = await _create.ProductAttribute.BuildAsync();
            var category = await _create.ProductCategory.BuildAsync();
            var status = await _create.ProductStatus.BuildAsync();

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Type = ProductType.Material,
                StatusId = status.Id,
                Name = "Test".WithGuid(),
                VendorCode = "Test",
                Price = 1,
                Image = null,
                IsHidden = true,
                IsDeleted = true,
                AttributeLinks = new List<ProductAttributeLink>
                {
                    new ()
                    {
                        ProductAttributeId = attribute.Id,
                        Value = "Test".WithGuid()
                    }
                },
                CategoryLinks = new List<ProductCategoryLink>
                {
                    new ()
                    {
                        ProductCategoryId = category.Id
                    }
                }
            };

            var createdProductId = await _productsClient.CreateAsync(product, headers);

            var createdProduct = await _productsClient.GetAsync(createdProductId, headers);

            Assert.NotNull(createdProduct);
            Assert.Equal(createdProductId, createdProduct.Id);
            Assert.Null(product.ParentProductId);
            Assert.Equal(product.Type, createdProduct.Type);
            Assert.Equal(product.StatusId, createdProduct.StatusId);
            Assert.Equal(product.Name, createdProduct.Name);
            Assert.Equal(product.VendorCode, createdProduct.VendorCode);
            Assert.Equal(product.Price, createdProduct.Price);
            Assert.Equal(product.IsHidden, createdProduct.IsHidden);
            Assert.Equal(product.IsDeleted, createdProduct.IsDeleted);
            Assert.True(createdProduct.CreateDateTime.IsMoreThanMinValue());
            Assert.NotEmpty(createdProduct.AttributeLinks);
            Assert.NotEmpty(createdProduct.CategoryLinks);
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(status.Id)
                .BuildAsync();
            var attribute = await _create.ProductAttribute.BuildAsync();
            var category = await _create.ProductCategory.BuildAsync();

            product.StatusId = status.Id;
            product.Type = ProductType.Material;
            product.Name = "Test".WithGuid();
            product.VendorCode = "Test";
            product.Price = 2;
            product.IsHidden = true;
            product.IsDeleted = true;
            product.AttributeLinks.Add(
                new ProductAttributeLink
                {
                    ProductAttributeId = attribute.Id,
                    Value = "Test".WithGuid()
                });
            product.CategoryLinks.Add(
                new ProductCategoryLink
                {
                    ProductCategoryId = category.Id
                });

            await _productsClient.UpdateAsync(product, headers);

            var updatedProduct = await _productsClient.GetAsync(product.Id, headers);

            Assert.Equal(product.StatusId, updatedProduct.StatusId);
            Assert.Equal(product.Type, updatedProduct.Type);
            Assert.Equal(product.Name, updatedProduct.Name);
            Assert.Equal(product.VendorCode, updatedProduct.VendorCode);
            Assert.Equal(product.Price, updatedProduct.Price);
            Assert.Equal(product.IsHidden, updatedProduct.IsHidden);
            Assert.Equal(product.IsDeleted, updatedProduct.IsDeleted);
            Assert.Equal(product.AttributeLinks.Single().Value, updatedProduct.AttributeLinks.Single().Value);
            Assert.Equal(
                product.AttributeLinks.Single().ProductAttributeId,
                updatedProduct.AttributeLinks.Single().ProductAttributeId);
            Assert.Equal(
                product.CategoryLinks.Single().ProductCategoryId,
                updatedProduct.CategoryLinks.Single().ProductCategoryId);
        }

        [Fact]
        public async Task WhenHide_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = await _create.ProductStatus.BuildAsync();
            var productIds = (
                    await Task.WhenAll(
                        _create.Product
                            .WithStatusId(status.Id)
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.Product
                            .WithStatusId(status.Id)
                            .WithName("Test".WithGuid()).BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _productsClient.HideAsync(productIds, headers);

            var products = await _productsClient.GetListAsync(productIds, headers);

            Assert.All(products, x => Assert.True(x.IsHidden));
        }

        [Fact]
        public async Task WhenShow_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = await _create.ProductStatus.BuildAsync();

            var productIds = (
                    await Task.WhenAll(
                        _create.Product
                            .WithStatusId(status.Id)
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.Product
                            .WithStatusId(status.Id)
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _productsClient.ShowAsync(productIds, headers);

            var products = await _productsClient.GetListAsync(productIds, headers);

            Assert.All(products, x => Assert.False(x.IsHidden));
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = await _create.ProductStatus.BuildAsync();
            var productIds = (
                    await Task.WhenAll(
                        _create.Product
                            .WithStatusId(status.Id)
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.Product
                            .WithStatusId(status.Id)
                            .WithName("Test".WithGuid())
                            .BuildAsync()))
                .Select(x => x.Id)
                .ToList();

            await _productsClient.DeleteAsync(productIds, headers);

            var products = await _productsClient.GetListAsync(productIds, headers);

            Assert.All(products, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var status = await _create.ProductStatus.BuildAsync();
            var productIds = (
                    await Task.WhenAll(
                        _create.Product
                            .WithStatusId(status.Id)
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.Product
                            .WithStatusId(status.Id)
                            .WithName("Test".WithGuid())
                            .BuildAsync()))
                .Select(x => x.Id)
                .ToList();

            await _productsClient.RestoreAsync(productIds, headers);

            var products = await _productsClient.GetListAsync(productIds, headers);

            Assert.All(products, x => Assert.False(x.IsDeleted));
        }
    }
}
