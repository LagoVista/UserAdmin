using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public interface IAuthRequestValidators
    {
        InvokeResult ValidateAccessTokenGrant(AuthRequest authRequest);
        InvokeResult ValidateAuthRequest(AuthRequest authRequest);
        InvokeResult ValidateRefreshTokenGrant(AuthRequest authRequest);
    }
}
