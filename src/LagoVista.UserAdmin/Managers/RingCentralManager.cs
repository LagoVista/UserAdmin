// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 51852a727edb2734644a87f25f47cf2be74c6414592f030a45f63b16c98ee591
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Phone;
using RingCentral;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class RingCentralManager : ICallLogManager
    {

        private readonly IRingCentralCredentials _rcCredentials;
        RestClient _rc;

        public RingCentralManager(IRingCentralCredentials rcCredentials)
        {
            _rcCredentials = rcCredentials ?? throw new ArgumentNullException(nameof(rcCredentials));
        }

        private async Task AuthAsync()
        {
            _rc = new RestClient(_rcCredentials.RingCentralClientId, _rcCredentials.RingCentralClientSecret, _rcCredentials.RingCentralUrl);
            await _rc.Authorize(_rcCredentials.RingCentralJWT);
        }

        public async Task<ListResponse<CallLog>> GetPhoneContactsAsync(string toPhoneNumber, ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthAsync();

            var records = new List<CallLog>();

            var callLogResponse = await _rc.Restapi().Account().CallLog().List(new ReadCompanyCallLogParameters
            {
                page = listRequest.PageIndex,
                perPage = listRequest.PageSize,
                dateFrom = DateTime.UtcNow.AddMonths(-6).ToString("o"),
                phoneNumber = toPhoneNumber
            });

            foreach (var record in callLogResponse.records)
            {
                var callLog = new CallLog()
                {
                    CallLogId = record.id,
                    DurationSeconds = record.duration,
                    TimeStamp = record.startTime,
                    FromName = record.from.name,
                    FromNumber = record.from.phoneNumber,
                    ToLocation = record.to.location,
                    ToName = record.to.name,
                    ToNumber = record.to.phoneNumber,
                    RecordingUrl = record.recording?.contentUri
                };

                records.Add(callLog);
            }

            return ListResponse<CallLog>.Create(listRequest, records);
        }
    }
}
