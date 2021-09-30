using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.DefaultRequestHeadersService;
using Crm.v1.Clients.Products.Clients;
using Crm.v1.Clients.Products.Models;

namespace Crm.Tests.All.Builders.Products
{
    public class ProductBuilder : IProductBuilder
    {
        private readonly IDefaultRequestHeadersService _defaultRequestHeadersService;
        private readonly IProductsClient _productsClient;
        private readonly Product _product;

        public ProductBuilder(
            IDefaultRequestHeadersService defaultRequestHeadersService,
            IProductsClient productsClient)
        {
            _productsClient = productsClient;
            _defaultRequestHeadersService = defaultRequestHeadersService;
            _product = new Product
            {
                Type = ProductType.Material,
                Name = "Test".WithGuid(),
                VendorCode = "Test",
                Price = 1,
                IsHidden = false,
                IsDeleted = false
            };
        }

        public ProductBuilder WithParentProductId(Guid productId)
        {
            _product.ParentProductId = productId;

            return this;
        }

        public ProductBuilder WithType(ProductType type)
        {
            _product.Type = type;

            return this;
        }

        public ProductBuilder WithStatusId(Guid statusId)
        {
            _product.StatusId = statusId;

            return this;
        }

        public ProductBuilder WithName(string name)
        {
            _product.Name = name;

            return this;
        }

        public ProductBuilder WithVendorCode(string vendorCode)
        {
            _product.VendorCode = vendorCode;

            return this;
        }

        public ProductBuilder WithPrice(decimal price)
        {
            _product.Price = price;

            return this;
        }

        public ProductBuilder AsHidden()
        {
            _product.IsHidden = true;

            return this;
        }

        public ProductBuilder AsDeleted()
        {
            _product.IsDeleted = true;

            return this;
        }

        public ProductBuilder WithAttributeLink(Guid attributeId, string value)
        {
            if (_product.AttributeLinks == null)
            {
                _product.AttributeLinks = new List<ProductAttributeLink>();
            }

            _product.AttributeLinks.Add(new ProductAttributeLink
            {
                ProductAttributeId = attributeId,
                Value = value
            });

            return this;
        }

        public ProductBuilder WithCategoryLink(Guid categoryId)
        {
            if (_product.CategoryLinks == null)
            {
                _product.CategoryLinks = new List<ProductCategoryLink>();
            }

            _product.CategoryLinks.Add(new ProductCategoryLink
            {
                ProductCategoryId = categoryId
            });

            return this;
        }

        public async Task<Product> BuildAsync()
        {
            var headers = await _defaultRequestHeadersService.GetAsync();

            if (_product.StatusId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_product.StatusId));
            }

            var id = await _productsClient.CreateAsync(_product, headers);

            return await _productsClient.GetAsync(id, headers);
        }
    }
}
