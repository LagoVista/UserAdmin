namespace LagoVista.UserAdmin.Models.Security.Passkeys
{
    public class PasskeyCredentialSummary
    {
        public string CredentialId { get; set; }
        public string Name { get; set; }
        public string CreatedUtc { get; set; }
        public string LastUsedUtc { get; set; }
    }
}
