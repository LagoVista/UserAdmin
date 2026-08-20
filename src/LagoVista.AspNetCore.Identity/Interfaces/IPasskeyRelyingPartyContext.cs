using System;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public sealed class PasskeyRelyingParty
    {
        public PasskeyRelyingParty(string rpId, string origin, bool isRequestScoped)
        {
            if (String.IsNullOrWhiteSpace(rpId)) throw new ArgumentNullException(nameof(rpId));
            if (String.IsNullOrWhiteSpace(origin)) throw new ArgumentNullException(nameof(origin));

            RpId = rpId;
            Origin = origin;
            IsRequestScoped = isRequestScoped;
        }

        public string RpId { get; }
        public string Origin { get; }
        public bool IsRequestScoped { get; }
    }

    public interface IPasskeyRelyingPartyContext
    {
        PasskeyRelyingParty Current { get; }
    }
}
