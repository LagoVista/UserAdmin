// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d6b633377be5a0d3b138d9e182790115ca39c9e69f3b141ecb686ace89f1968d
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{
    public sealed class TwitterErrorResponse
    {
        public List<TwitterError> Errors { get; set; }
    }

    public sealed class TwitterError
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }
}
