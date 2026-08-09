using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.AnonymousVisitors
{
    internal interface IAnonymousVisitorStateIndexRepo
    {
        Task InsertAsync(AnonymousVisitorState state, DateTime dueUtc, string actorId);
        Task<bool> ExistsAsync(AnonymousVisitorState state, DateTime dueUtc, string actorId);
        Task<IEnumerable<string>> FindActorIdsAsync(AnonymousVisitorState state, DateTime? dueBeforeUtc, int take);
        Task DeleteAsync(AnonymousVisitorState state, DateTime dueUtc, string actorId);
    }
}
