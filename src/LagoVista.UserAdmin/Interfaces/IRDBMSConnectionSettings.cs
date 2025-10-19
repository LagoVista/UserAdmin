// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 144528eaf2f609ab69b01941da01e81b2f20eb4b8a711f3f1e5dbc91976d010b
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Interfaces;

namespace LagoVista.UserAdmin
{
    public interface IRDBMSConnectionSettings
    {
        IConnectionSettings DbConnectionSettings { get;  }
    }
}
