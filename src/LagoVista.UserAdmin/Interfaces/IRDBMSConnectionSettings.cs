using LagoVista.Core.Interfaces;

namespace LagoVista.UserAdmin
{
    public interface IRDBMSConnectionSettings
    {
        IConnectionSettings DbConnectionSettings { get;  }
    }
}
