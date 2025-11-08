// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 178ffa2dcb93efde4daab05ef036bdd9ca421cfbb3fe5fa342c1804e2146b07b
// IndexVersion: 2
// --- END CODE INDEX META ---
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
