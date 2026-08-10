using System;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public interface IAnonymousVisitorBootstrapOptions
    {
        TimeSpan ActiveLifetime { get; }
    }
}
