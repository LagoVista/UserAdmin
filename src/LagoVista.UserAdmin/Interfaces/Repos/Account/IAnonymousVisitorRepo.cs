using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Users
{
    public interface IAnonymousVisitorRepo
    {
        Task CreateAsync(AnonymousVisitor visitor);
        Task<AnonymousVisitor> GetByActorIdAsync(string actorId);
        Task<AnonymousVisitor> FindByContinuityTokenHashAsync(string continuityTokenHash);
        Task<AnonymousVisitor> FindByInstallationIdHashAsync(string installationIdHash);
        Task<IEnumerable<AnonymousVisitor>> GetByStateAsync(AnonymousVisitorState state, DateTime? dueBeforeUtc = null, int take = 100);
        Task UpdateAsync(AnonymousVisitor visitor);
        Task DeleteAsync(string actorId);
    }
}
