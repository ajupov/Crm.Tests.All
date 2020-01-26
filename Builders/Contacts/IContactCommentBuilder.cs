using System;
using System.Threading.Tasks;

namespace Crm.Tests.All.Builders.Contacts
{
    public interface IContactCommentBuilder
    {
        ContactCommentBuilder WithContactId(Guid contactId);

        Task BuildAsync();
    }
}