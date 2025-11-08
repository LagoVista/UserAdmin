// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: cb3d865e5d0afd0f1eaf104c93b46c754315244f616cce57c60088edfc6792de
// IndexVersion: 2
// --- END CODE INDEX META ---
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
