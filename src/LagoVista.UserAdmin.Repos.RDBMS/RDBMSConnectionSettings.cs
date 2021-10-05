using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces;
using System;

namespace LagoVista.UserAdmin.Repos.RDBMS
{
    public class RDBMSConnectionSettings : IRDBMSConnectionSettings
    {
        public RDBMSConnectionSettings(IConnectionSettings connectionSettings)
        {
            DbConnectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
        }

        public IConnectionSettings DbConnectionSettings { get; }
    }
}
