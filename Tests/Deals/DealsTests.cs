using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ajupov.Utils.All.DateTime;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.Tests.All.Services.Creator;
using Crm.v1.Clients.Deals.Clients;
using Crm.v1.Clients.Deals.Models;
using Crm.v1.Clients.Deals.Requests;
using Xunit;

namespace Crm.Tests.All.Tests.Deals
{
    public class DealTests
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly ICreate _create;
        private readonly IDealsClient _dealsClient;

        public DealTests(IAccessTokenGetter accessTokenGetter, ICreate create, IDealsClient dealsClient)
        {
            _accessTokenGetter = accessTokenGetter;
            _create = create;
            _dealsClient = dealsClient;
        }

        [Fact]
        public async Task WhenGet_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.DealType.BuildAsync();
            var status = await _create.DealStatus.BuildAsync();
            var dealId = (
                    await _create.Deal
                        .WithTypeId(type.Id)
                        .WithStatusId(status.Id)
                        .BuildAsync())
                .Id;

            var deal = await _dealsClient.GetAsync(accessToken, dealId);

            Assert.NotNull(deal);
            Assert.Equal(dealId, deal.Id);
        }

        [Fact]
        public async Task WhenGetList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.DealType.BuildAsync();
            var status = await _create.DealStatus.BuildAsync();
            var dealIds = (
                    await Task.WhenAll(
                        _create.Deal
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync(),
                        _create.Deal
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            var deals = await _dealsClient.GetListAsync(accessToken, dealIds);

            Assert.NotEmpty(deals);
            Assert.Equal(dealIds.Count, deals.Count);
        }

        [Fact]
        public async Task WhenGetPagedList_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attribute = await _create.DealAttribute.BuildAsync();
            var type = await _create.DealType.BuildAsync();
            var status = await _create.DealStatus.BuildAsync();
            var value = "Test".WithGuid();
            await Task.WhenAll(
                _create.Deal
                    .WithTypeId(type.Id)
                    .WithStatusId(status.Id)
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync(),
                _create.Deal
                    .WithTypeId(type.Id)
                    .WithStatusId(status.Id)
                    .WithAttributeLink(attribute.Id, value)
                    .BuildAsync());
            var filterAttributes = new Dictionary<Guid, string> {{attribute.Id, value}};
            var filterSourceIds = new List<Guid> {status.Id};

            var request = new DealGetPagedListRequest
            {
                Attributes = filterAttributes,
                StatusIds = filterSourceIds
            };

            var response = await _dealsClient.GetPagedListAsync(accessToken, request);

            var results = response.Deals
                .Skip(1)
                .Zip(response.Deals, (previous, current) => current.CreateDateTime >= previous.CreateDateTime);

            Assert.NotEmpty(response.Deals);
            Assert.All(results, Assert.True);
        }

        [Fact]
        public async Task WhenCreate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var attribute = await _create.DealAttribute.BuildAsync();
            var type = await _create.DealType.BuildAsync();
            var dealStatus = await _create.DealStatus.BuildAsync();
            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();

            var deal = new Deal
            {
                TypeId = type.Id,
                StatusId = dealStatus.Id,
                Name = "Test".WithGuid(),
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddDays(1),
                Sum = 1,
                SumWithoutDiscount = 1,
                FinishProbability = 50,
                IsDeleted = true,
                Positions = new List<DealPosition>
                {
                    new DealPosition
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Count = 1,
                        Price = product.Price
                    }
                },
                AttributeLinks = new List<DealAttributeLink>
                {
                    new DealAttributeLink
                    {
                        DealAttributeId = attribute.Id,
                        Value = "Test".WithGuid()
                    }
                }
            };

            var createdDealId = await _dealsClient.CreateAsync(accessToken, deal);

            var createdDeal = await _dealsClient.GetAsync(accessToken, createdDealId);

            Assert.NotNull(createdDeal);
            Assert.Equal(createdDealId, createdDeal.Id);
            Assert.Equal(deal.TypeId, createdDeal.TypeId);
            Assert.Equal(deal.StatusId, createdDeal.StatusId);
            Assert.Equal(deal.CompanyId, createdDeal.CompanyId);
            Assert.Equal(deal.ContactId, createdDeal.ContactId);
            Assert.True(!createdDeal.CreateUserId.IsEmpty());
            Assert.Equal(deal.ResponsibleUserId, createdDeal.ResponsibleUserId);
            Assert.Equal(deal.Name, createdDeal.Name);
            Assert.Equal(deal.StartDateTime.Date, createdDeal.StartDateTime.Date);
            Assert.Equal(deal.EndDateTime?.Date, createdDeal.EndDateTime?.Date);
            Assert.Equal(deal.Sum, createdDeal.Sum);
            Assert.Equal(deal.SumWithoutDiscount, createdDeal.SumWithoutDiscount);
            Assert.Equal(deal.FinishProbability, createdDeal.FinishProbability);
            Assert.Equal(deal.IsDeleted, createdDeal.IsDeleted);
            Assert.True(createdDeal.CreateDateTime.IsMoreThanMinValue());
            Assert.NotEmpty(createdDeal.Positions);
            Assert.NotEmpty(createdDeal.AttributeLinks);
        }

        [Fact]
        public async Task WhenUpdate_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.DealType.BuildAsync();
            var dealStatus = await _create.DealStatus.BuildAsync();
            var productStatus = await _create.ProductStatus.BuildAsync();
            var product = await _create.Product
                .WithStatusId(productStatus.Id)
                .BuildAsync();
            var attribute = await _create.DealAttribute.BuildAsync();
            var deal = await _create.Deal
                .WithTypeId(type.Id)
                .WithStatusId(dealStatus.Id)
                .BuildAsync();

            deal.TypeId = type.Id;
            deal.StatusId = dealStatus.Id;
            deal.Name = "Test".WithGuid();
            deal.StartDateTime = DateTime.UtcNow;
            deal.EndDateTime = DateTime.UtcNow.AddDays(1);
            deal.Sum = 1;
            deal.SumWithoutDiscount = 1;
            deal.FinishProbability = 50;
            deal.IsDeleted = true;
            deal.Positions.Add(
                new DealPosition
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Count = 1,
                    Price = product.Price
                });
            deal.AttributeLinks.Add(
                new DealAttributeLink
                {
                    DealAttributeId = attribute.Id,
                    Value = "Test".WithGuid()
                });

            await _dealsClient.UpdateAsync(accessToken, deal);

            var updatedDeal = await _dealsClient.GetAsync(accessToken, deal.Id);

            Assert.Equal(deal.AccountId, updatedDeal.AccountId);
            Assert.Equal(deal.TypeId, updatedDeal.TypeId);
            Assert.Equal(deal.StatusId, updatedDeal.StatusId);
            Assert.Equal(deal.CompanyId, updatedDeal.CompanyId);
            Assert.Equal(deal.ContactId, updatedDeal.ContactId);
            Assert.Equal(deal.CreateUserId, updatedDeal.CreateUserId);
            Assert.Equal(deal.ResponsibleUserId, updatedDeal.ResponsibleUserId);
            Assert.Equal(deal.Name, updatedDeal.Name);
            Assert.Equal(deal.StartDateTime.Date, updatedDeal.StartDateTime.Date);
            Assert.Equal(deal.EndDateTime?.Date, updatedDeal.EndDateTime?.Date);
            Assert.Equal(deal.Sum, updatedDeal.Sum);
            Assert.Equal(deal.SumWithoutDiscount, updatedDeal.SumWithoutDiscount);
            Assert.Equal(deal.FinishProbability, updatedDeal.FinishProbability);
            Assert.Equal(deal.IsDeleted, updatedDeal.IsDeleted);
            Assert.Equal(deal.Positions.Single().ProductId, updatedDeal.Positions.Single().ProductId);
            Assert.Equal(deal.Positions.Single().Count, updatedDeal.Positions.Single().Count);
            Assert.Equal(deal.Positions.Single().Price, updatedDeal.Positions.Single().Price);
            Assert.Equal(deal.AttributeLinks.Single().Value, updatedDeal.AttributeLinks.Single().Value);
            Assert.Equal(
                deal.AttributeLinks.Single().DealAttributeId,
                updatedDeal.AttributeLinks.Single().DealAttributeId);
        }

        [Fact]
        public async Task WhenDelete_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.DealType.BuildAsync();
            var status = await _create.DealStatus.BuildAsync();
            var dealIds = (
                    await Task.WhenAll(
                        _create.Deal
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync(),
                        _create.Deal
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _dealsClient.DeleteAsync(accessToken, dealIds);

            var deals = await _dealsClient.GetListAsync(accessToken, dealIds);

            Assert.All(deals, x => Assert.True(x.IsDeleted));
        }

        [Fact]
        public async Task WhenRestore_ThenSuccess()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            var type = await _create.DealType.BuildAsync();
            var status = await _create.DealStatus.BuildAsync();
            var dealIds = (
                    await Task.WhenAll(
                        _create.Deal
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync(),
                        _create.Deal
                            .WithTypeId(type.Id)
                            .WithStatusId(status.Id)
                            .BuildAsync())
                )
                .Select(x => x.Id)
                .ToList();

            await _dealsClient.RestoreAsync(accessToken, dealIds);

            var deals = await _dealsClient.GetListAsync(accessToken, dealIds);

            Assert.All(deals, x => Assert.False(x.IsDeleted));
        }
    }
}