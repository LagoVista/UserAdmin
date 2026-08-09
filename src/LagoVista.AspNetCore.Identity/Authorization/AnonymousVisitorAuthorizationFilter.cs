using LagoVista.AspNetCore.Identity.Managers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Authorization
{
    public sealed class AnonymousVisitorAuthorizationFilter : IAsyncAuthorizationFilter
    {
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var identityStage = context.HttpContext.User?.FindFirst(ClaimsFactory.IdentityStage)?.Value;
            var visitorDenied = String.Equals(identityStage, ClaimsFactory.VisitorIdentityStage, StringComparison.Ordinal) && !context.Filters.OfType<AllowAnonymousVisitorAttribute>().Any();
            var provisionalDenied = String.Equals(identityStage, ClaimsFactory.ProvisionalIdentityStage, StringComparison.Ordinal) && !context.Filters.OfType<AllowProvisionalIdentityAttribute>().Any();
            if (visitorDenied || provisionalDenied)
                context.Result = new ForbidResult();

            return Task.CompletedTask;
        }
    }
}
