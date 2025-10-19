// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 02b42c8c014c83bb83df644595ef869e37307afa7dc6320ca012f9a9096fbac4
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin
{
    public interface IRingCentralCredentials
    {
        public string RingCentralClientId { get; }
        public string RingCentralClientSecret { get; }
        public string RingCentralJWT { get; }
        public string RingCentralUrl { get; }
    }
}
