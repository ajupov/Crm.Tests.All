using System;
using System.Threading.Tasks;
using Crm.v1.Clients.Orders.Models;

namespace Crm.Tests.All.Builders.Orders
{
    public interface IOrderBuilder
    {
        OrderBuilder WithTypeId(Guid typeId);

        OrderBuilder WithStatusId(Guid statusId);

        OrderBuilder WithCreateUserId(Guid createUserId);

        OrderBuilder WithResponsibleUserId(Guid responsibleUserId);

        OrderBuilder WithCustomer(Guid customerId);

        OrderBuilder WithName(string name);

        OrderBuilder WithStartDateTime(DateTime startDateTime);

        OrderBuilder WithEndDateTime(DateTime endDateTime);

        OrderBuilder WithSum(decimal sum);

        OrderBuilder WithSumWithoutDiscount(decimal sumWithoutDiscount);

        OrderBuilder AsDeleted();

        OrderBuilder WithItem(
            Guid productId,
            string productName,
            string productVendorCode,
            decimal price,
            decimal count);

        OrderBuilder WithAttributeLink(Guid attributeId, string value);

        Task<Order> BuildAsync();
    }
}
