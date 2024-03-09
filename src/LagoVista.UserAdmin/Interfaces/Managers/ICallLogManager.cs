using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Phone;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface ICallLogManager
    {
        Task<ListResponse<CallLog>> GetPhoneContactsAsync(string toPhoneNumber, ListRequest listRequest, EntityHeader org, EntityHeader user);
    }
}
