using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
	public class ClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, Role>
	{

		IClaimsFactory _claimsFactory;

		public ClaimsPrincipalFactory(UserManager<AppUser> userManager, RoleManager<Role> roleManager, IClaimsFactory claimsFactory, IOptions<IdentityOptions> optionsAccessor) : base(userManager, roleManager, optionsAccessor)
		{
			_claimsFactory = claimsFactory;
		}

		public async override Task<ClaimsPrincipal> CreateAsync(AppUser user)
		{
			var principal = await base.CreateAsync(user);

			((ClaimsIdentity)principal.Identity).AddClaims(_claimsFactory.GetClaims(user).ToArray());

			return principal;
		}

		protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
		{
			try
			{
				return await base.GenerateClaimsAsync(user);
			}
			catch (Exception)
			{
				if (string.IsNullOrWhiteSpace(user.Email) || !user.Email.Contains("@"))
				{
					try
					{
						var userId = user.Id;
						var userNameAsync = user.Name;
						var id = new ClaimsIdentity("Identity.Application", this.Options.ClaimsIdentity.UserNameClaimType, this.Options.ClaimsIdentity.RoleClaimType);
						id.AddClaim(new Claim(this.Options.ClaimsIdentity.UserIdClaimType, userId));
						id.AddClaim(new Claim(this.Options.ClaimsIdentity.UserNameClaimType, userNameAsync));
						return id;
					}
					catch
					{
						throw;
					}
				}
				else
				{
					throw;
				}
			}
		}
	}
}
