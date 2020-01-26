using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ajupov.Utils.All.Guid;
using Crm.Tests.All.Extensions;
using Crm.Tests.All.Services.AccessTokenGetter;
using Crm.v1.Clients.Deals.Clients;
using Crm.v1.Clients.Deals.Models;

namespace Crm.Tests.All.Builders.Deals
{
    public class DealBuilder : IDealBuilder
    {
        private readonly IAccessTokenGetter _accessTokenGetter;
        private readonly IDealsClient _dealsClient;
        private readonly Deal _deal;

        public DealBuilder(IAccessTokenGetter accessTokenGetter, IDealsClient dealsClient)
        {
            _dealsClient = dealsClient;
            _accessTokenGetter = accessTokenGetter;
            _deal = new Deal
            {
                Name = "Test".WithGuid(),
                StartDateTime = DateTime.UtcNow,
                EndDateTime = DateTime.UtcNow.AddDays(1),
                Sum = 1,
                SumWithoutDiscount = 1,
                FinishProbability = 50,
                IsDeleted = false
            };
        }

        public DealBuilder WithTypeId(Guid typeId)
        {
            _deal.TypeId = typeId;

            return this;
        }

        public DealBuilder WithStatusId(Guid statusId)
        {
            _deal.StatusId = statusId;

            return this;
        }

        public DealBuilder WithCompanyId(Guid companyId)
        {
            _deal.CompanyId = companyId;

            return this;
        }

        public DealBuilder WithContactId(Guid contactId)
        {
            _deal.ContactId = contactId;

            return this;
        }

        public DealBuilder WithCreateUserId(Guid createUserId)
        {
            _deal.CreateUserId = createUserId;

            return this;
        }

        public DealBuilder WithResponsibleUserId(Guid responsibleUserId)
        {
            _deal.ResponsibleUserId = responsibleUserId;

            return this;
        }

        public DealBuilder WithName(string name)
        {
            _deal.Name = name;

            return this;
        }

        public DealBuilder WithStartDateTime(DateTime startDateTime)
        {
            _deal.StartDateTime = startDateTime;

            return this;
        }

        public DealBuilder WithEndDateTime(DateTime endDateTime)
        {
            _deal.EndDateTime = endDateTime;

            return this;
        }

        public DealBuilder WithSum(decimal sum)
        {
            _deal.Sum = sum;

            return this;
        }

        public DealBuilder WithSumWithoutDiscount(decimal sumWithoutDiscount)
        {
            _deal.SumWithoutDiscount = sumWithoutDiscount;

            return this;
        }

        public DealBuilder WithFinishProbability(byte finishProbability)
        {
            _deal.FinishProbability = finishProbability;

            return this;
        }

        public DealBuilder AsDeleted()
        {
            _deal.IsDeleted = true;

            return this;
        }

        public DealBuilder WithPosition(
            Guid productId,
            string productName,
            string productVendorCode,
            decimal price,
            decimal count)
        {
            if (_deal.Positions == null)
            {
                _deal.Positions = new List<DealPosition>();
            }

            _deal.Positions.Add(new DealPosition
            {
                ProductId = productId,
                ProductName = productName,
                ProductVendorCode = productVendorCode,
                Price = price,
                Count = count
            });

            return this;
        }

        public DealBuilder WithAttributeLink(Guid attributeId, string value)
        {
            if (_deal.AttributeLinks == null)
            {
                _deal.AttributeLinks = new List<DealAttributeLink>();
            }

            _deal.AttributeLinks.Add(new DealAttributeLink
            {
                DealAttributeId = attributeId,
                Value = value
            });

            return this;
        }

        public async Task<Deal> BuildAsync()
        {
            var accessToken = await _accessTokenGetter.GetAsync();

            if (_deal.TypeId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_deal.TypeId));
            }

            if (_deal.StatusId.IsEmpty())
            {
                throw new InvalidOperationException(nameof(_deal.StatusId));
            }

            var id = await _dealsClient.CreateAsync(accessToken, _deal);

            return await _dealsClient.GetAsync(accessToken, id);
        }
    }
}