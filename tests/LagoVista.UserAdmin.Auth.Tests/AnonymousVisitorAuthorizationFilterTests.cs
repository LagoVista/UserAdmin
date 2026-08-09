using LagoVista.AspNetCore.Identity.Authorization;
using LagoVista.AspNetCore.Identity.Managers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class AnonymousVisitorAuthorizationFilterTests
    {
        [Test]
        public async Task OnAuthorizationAsync_Should_Forbid_Visitor_From_Unmarked_Endpoint()
        {
            var context = CreateContext(CreateVisitorPrincipal(), new List<IFilterMetadata>());

            await new AnonymousVisitorAuthorizationFilter().OnAuthorizationAsync(context);

            Assert.That(context.Result, Is.TypeOf<ForbidResult>());
        }

        [Test]
        public async Task OnAuthorizationAsync_Should_Allow_Visitor_On_Marked_Endpoint()
        {
            var filters = new List<IFilterMetadata> { new AllowAnonymousVisitorAttribute() };
            var context = CreateContext(CreateVisitorPrincipal(), filters);

            await new AnonymousVisitorAuthorizationFilter().OnAuthorizationAsync(context);

            Assert.That(context.Result, Is.Null);
        }

        [Test]
        public async Task OnAuthorizationAsync_Should_Not_Restrict_NonVisitor_Identity()
        {
            var claims = new[] { new Claim(ClaimsFactory.CurrentUserId, "established-user-id") };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
            var context = CreateContext(principal, new List<IFilterMetadata>());

            await new AnonymousVisitorAuthorizationFilter().OnAuthorizationAsync(context);

            Assert.That(context.Result, Is.Null);
        }

        private static ClaimsPrincipal CreateVisitorPrincipal()
        {
            var claims = new[] { new Claim(ClaimsFactory.IdentityStage, ClaimsFactory.VisitorIdentityStage) };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        }

        private static AuthorizationFilterContext CreateContext(ClaimsPrincipal principal, IList<IFilterMetadata> filters)
        {
            var httpContext = new DefaultHttpContext { User = principal };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            return new AuthorizationFilterContext(actionContext, filters);
        }
    }
}
