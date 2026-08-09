using System;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public interface IAnonymousVisitorBootstrapOptions
    {
        string AppUserId { get; }
        string OrganizationId { get; }
        TimeSpan ActiveLifetime { get; }
    }
}
