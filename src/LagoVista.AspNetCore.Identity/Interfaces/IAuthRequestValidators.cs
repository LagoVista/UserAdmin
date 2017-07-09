using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public interface IAuthRequestValidators
    {
        InvokeResult ValidateAccessTokenGrant(AuthRequest authRequest);
        InvokeResult ValidateAuthRequest(AuthRequest authRequest);
        InvokeResult ValidateRefreshTokenGrant(AuthRequest authRequest);
        Task<InvokeResult> ValidateRefreshTokenAsync(string refreshTokenId, string userId);
        InvokeResult ValidateRefreshTokenFormat(string refreshToken);
    }
}
