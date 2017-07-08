using LagoVista.Core.Validation;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IEmailSender
    {
        Task<InvokeResult> SendAsync(string email, string subject, string message);
    }
}
