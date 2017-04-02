using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IEmailSender
    {
        Task SendAsync(string email, string subject, string message);
    }
}
