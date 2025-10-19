// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 528a58ef6743ed85073ab05f649821f39dad3fb8b81d8f43848308abfdadebbf
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IPasswordManager
    {
        Task<InvokeResult> ResetPasswordAsync(ResetPassword resetPassword);
        Task<InvokeResult> ChangePasswordAsync(ChangePassword changePasswordDTO, EntityHeader orgEntityHeader, EntityHeader userEntityHeader);
        Task<InvokeResult> SendResetPasswordLinkAsync(SendResetPasswordLink sendResetPasswordLink);
    }
}
