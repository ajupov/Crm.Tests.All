using System;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.Creator;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;
using Crm.v1.Clients.Products.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Products
{
    public class ProductCategoriesTests
    {
        private readonly ICreate _create;
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IProductCategoriesClient _productCategoriesClient;

        public ProductCategoriesTests(
            ICreate create,
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IProductCategoriesClient productCategoriesClient)
        {
            _create = create;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _productCategoriesClient = productCategoriesClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var categoriesId = (await _create.ProductCategory.BuildAsync()).Id;

            var categories = await _productCategoriesClient.GetAsync(categoriesId, headers);

            Assert.NotNull(categories);
            Assert.Equal(categoriesId, categories.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var categoriesIds = (
                    await Task.WhenAll(
                        _create.ProductCategory
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.ProductCategory
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var categories = await _productCategoriesClient.GetListAsync(categoriesIds, headers);

            Assert.NotEmpty(categories);
            Assert.Equal(categoriesIds.Count, categories.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var name = "Test".WithGuid();
            await Task.WhenAll(_create.ProductCategory
                .WithName(name)
                .BuildAsync());

            var request = new ProductCategoryGetPagedListRequest
            {
                Name = name
            };

            var response = await _productCategoriesClient.GetPagedListAsync(request, headers);

            var results = response.Categories
                .Skip(1)
                .Zip(response.Categories, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Categories);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var categories = new ProductCategory
            {
                Id = Guid.NewGuid(),
                Name = "Test".WithGuid(),
                IsDeleted = false
            };

            var createdCategoryId = await _productCategoriesClient.CreateAsync(categories, headers);

            var createdCategory = await _productCategoriesClient.GetAsync(createdCategoryId, headers);

            Assert.NotNull(createdCategory);
            Assert.Equal(createdCategoryId, createdCategory.Id);
            Assert.Equal(categories.Name, createdCategory.Name);
            Assert.Equal(categories.IsDeleted, createdCategory.IsDeleted);
            Assert.True(createdCategory.CreateDateTime.IsMoreThanMinValue());
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var categories = await _create.ProductCategory
                .WithName("Test".WithGuid())
                .BuildAsync();

            categories.Name = "Test".WithGuid();
            categories.IsDeleted = true;

            await _productCategoriesClient.UpdateAsync(categories, headers);

            var updatedCategory = await _productCategoriesClient.GetAsync(categories.Id, headers);

            Assert.Equal(categories.Name, updatedCategory.Name);
            Assert.Equal(categories.IsDeleted, updatedCategory.IsDeleted);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var categoriesIds = (
                    await Task.WhenAll(
                        _create.ProductCategory
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.ProductCategory
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _productCategoriesClient.DeleteAsync(categoriesIds, headers);

            var categories = await _productCategoriesClient.GetListAsync(categoriesIds, headers);

            Assert.All(categories, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            var categoriesIds = (
                    await Task.WhenAll(
                        _create.ProductCategory
                            .WithName("Test".WithGuid())
                            .BuildAsync(),
                        _create.ProductCategory
                            .WithName("Test".WithGuid())
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _productCategoriesClient.RestoreAsync(categoriesIds, headers);

            var categories = await _productCategoriesClient.GetListAsync(categoriesIds, headers);

            Assert.All(categories, x => Assert.False(x.IsDeleted));
        }
    }
}
