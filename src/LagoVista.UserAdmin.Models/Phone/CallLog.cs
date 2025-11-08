// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 84ac933501f357e36083ba4149c86c38881ffe89ce09e5a099852fddcdc7cbd9
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Phone
{
    public class CallLog
    {
        public string CallLogId { get; set; }

        public string TimeStamp { get; set; }
        public string FromNumber { get; set; }
        public string FromName { get; set; }
        public string ToNumber { get; set; }
        public string ToName { get; set; }
        public string ToLocation { get; set; }

        public long? DurationSeconds { get; set; }
        public string RecordingUrl { get; set; }

        public override string ToString()
        {
            return $"{CallLogId} {TimeStamp} - {FromName} {FromNumber} {ToNumber} {DurationSeconds} seconds - {RecordingUrl}";
        }
    }
}
