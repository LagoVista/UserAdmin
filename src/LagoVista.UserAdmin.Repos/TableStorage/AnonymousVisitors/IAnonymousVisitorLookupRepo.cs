using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.AnonymousVisitors
{
    internal interface IAnonymousVisitorLookupRepo
    {
        Task InsertAsync(string lookupHash, string actorId, DateTime createdUtc);
        Task<string> FindActorIdAsync(string lookupHash);
        Task DeleteAsync(string lookupHash);
    }

    internal interface IAnonymousVisitorContinuityIndexRepo : IAnonymousVisitorLookupRepo
    {
    }

    internal interface IAnonymousVisitorInstallationIndexRepo : IAnonymousVisitorLookupRepo
    {
    }
}
