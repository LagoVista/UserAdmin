using System;

namespace LagoVista.UserAdmin.Models.Users
{
    public class EstablishProvisionalAccountRequest
    {
        public string ProvisionalEnvironmentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class EstablishProvisionalAccountResponse
    {
        public string ProvisionalEnvironmentId { get; set; }
        public string AppUserId { get; set; }
        public string OrganizationId { get; set; }
        public string Email { get; set; }
        public bool EmailVerificationRequired { get; set; }
        public string DevelopmentVerificationCode { get; set; }
    }
}
