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
