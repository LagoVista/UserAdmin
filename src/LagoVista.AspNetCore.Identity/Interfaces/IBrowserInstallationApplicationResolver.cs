using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    /// <summary>
    /// Supplies verified application identity for the current HTTPS origin.
    /// Implemented by the library owning canonical application records.
    /// No request-supplied app or tenant identity may establish this binding.
    /// </summary>
    public interface IBrowserInstallationApplicationResolver
    {
        Task<(string AppId, Uri Origin)> ResolveAsync(HttpContext context);
    }
}
