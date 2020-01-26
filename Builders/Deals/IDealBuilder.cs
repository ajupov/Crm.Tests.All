using System;
using System.Threading.Tasks;
using Crm.v1.Clients.Deals.Models;

namespace Crm.Tests.All.Builders.Deals
{
    public interface IDealBuilder
    {
        DealBuilder WithTypeId(Guid typeId);

        DealBuilder WithStatusId(Guid statusId);

        DealBuilder WithCompanyId(Guid companyId);

        DealBuilder WithContactId(Guid contactId);

        DealBuilder WithCreateUserId(Guid createUserId);

        DealBuilder WithResponsibleUserId(Guid responsibleUserId);

        DealBuilder WithName(string name);

        DealBuilder WithStartDateTime(DateTime startDateTime);

        DealBuilder WithEndDateTime(DateTime endDateTime);

        DealBuilder WithSum(decimal sum);

        DealBuilder WithSumWithoutDiscount(decimal sumWithoutDiscount);

        DealBuilder WithFinishProbability(byte finishProbability);

        DealBuilder AsDeleted();

        DealBuilder WithPosition(
            Guid productId,
            string productName,
            string productVendorCode,
            decimal price,
            decimal count);

        DealBuilder WithAttributeLink(Guid attributeId, string value);

        Task<Deal> BuildAsync();
    }
}