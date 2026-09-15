namespace LagoVista.AspNetCore.AuthorizationServer.Persistence.Cassandra
{
    /// <summary>
    /// Short-lived OpenIddict protocol token persisted in Cassandra.
    /// </summary>
    public class OpenIddictProtocolToken
    {
        public string Id { get; set; }
        public string ApplicationId { get; set; }
        public string AuthorizationId { get; set; }
        public string Subject { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string ReferenceId { get; set; }
        public string Payload { get; set; }
        public string PropertiesJson { get; set; }
        public string CreationDateUtc { get; set; }
        public string ExpirationDateUtc { get; set; }
        public string RedemptionDateUtc { get; set; }
        public long Version { get; set; }
    }
}
