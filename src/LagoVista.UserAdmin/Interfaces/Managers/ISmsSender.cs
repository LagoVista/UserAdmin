using LagoVista.Core.Validation;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface ISmsSender
    {
        Task<InvokeResult> SendAsync(string number, string message);
        Task<InvokeResult> SendInBackgroundAsync(string number, string contents);
    }
}
