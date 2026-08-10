using System;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public interface IAnonymousVisitorTokenService
    {
        string CreateToken(string actorId, DateTime accessExpiresUtc);
    }
}
