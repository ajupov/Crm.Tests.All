using System;
using System.Threading.Tasks;
using Crm.v1.Clients.Suppliers.Models;

namespace Crm.Tests.All.Builders.Suppliers
{
    public interface ISupplierBuilder
    {
        SupplierBuilder WithCreateUserId(Guid createUserId);

        SupplierBuilder WithName(string name);

        SupplierBuilder WithPhone(string phone);

        SupplierBuilder WithEmail(string email);

        SupplierBuilder AsDeleted();

        SupplierBuilder WithAttributeLink(Guid attributeId, string value);

        Task<Supplier> BuildAsync();
    }
}
