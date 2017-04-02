using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface ISmsSender
    {
        Task SendAsync(string number, string message);
    }
}
