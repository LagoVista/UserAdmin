using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace LagoVista.AspNetCore.Identity.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AllowProvisionalIdentityAttribute : Attribute, IFilterMetadata
    {
    }
}
