using System.Threading.Tasks;
using Crm.Common.All.Types.AttributeType;
using Crm.V1.Clients.Contacts.Models;

namespace Crm.Tests.All.Builders.Contacts
{
    public interface IContactAttributeBuilder
    {
        ContactAttributeBuilder WithType(AttributeType type);

        ContactAttributeBuilder WithKey(string key);

        ContactAttributeBuilder AsDeleted();

        Task<ContactAttribute> BuildAsync();
    }
}
