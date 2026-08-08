using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using System;
using System.Security.Cryptography;
using System.Text;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal class ProvisionalEnvironmentCreationIndexEntity : TableStorageEntity
    {
        public string CreationRequestId { get; set; }
        public string EnvironmentId { get; set; }
        public string CreatedUtc { get; set; }

        public static string CreatePartitionKey(string creationRequestId)
        {
            var lookupKey = CreateLookupKey(creationRequestId);
            return $"CRT|{lookupKey.Substring(0, 2)}";
        }

        public static string CreateRowKey(string creationRequestId)
        {
            return $"CRT|{CreateLookupKey(creationRequestId)}";
        }

        private static string CreateLookupKey(string creationRequestId)
        {
            if (String.IsNullOrEmpty(creationRequestId)) throw new ArgumentNullException(nameof(creationRequestId));

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(creationRequestId));
                var builder = new StringBuilder(hash.Length * 2);
                foreach (var value in hash) builder.Append(value.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
