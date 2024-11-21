using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using Prometheus;
using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace LagoVista.AspNetCore.Identity.Services
{
    public class TwilioSMSSender : ISmsSender
    {

        protected static readonly Counter SendSMSMessage = Metrics.CreateCounter("nuviot_send_sms", "Send SMS Message to a user account", "entity");

        ILagoVistaAspNetCoreIdentityProviderSettings _settings;
        IAdminLogger _adminLogger;
        private readonly IBackgroundServiceTaskQueue _taskQueue;
        public TwilioSMSSender(ILagoVistaAspNetCoreIdentityProviderSettings settings, IBackgroundServiceTaskQueue taskQueue, IAdminLogger adminLogger)
        {
            _settings = settings;
            _adminLogger = adminLogger;
            _taskQueue = taskQueue;
        }

        public async Task<InvokeResult> SendInBackgroundAsync(string number, string contents)
        {
            await _taskQueue.QueueBackgroundWorkItemAsync(async (token) =>
            {
                await SendAsync(number, contents);
            });

            return InvokeResult.Success;
        }


        public async Task<InvokeResult> SendAsync(string number, string contents)
        {
            try
            {
                TwilioClient.Init(_settings.SmsServer.AccountId, _settings.SmsServer.AccessKey);
                var restClient = new TwilioRestClient(_settings.SmsServer.AccountId, _settings.SmsServer.AccessKey);
                await MessageResource.CreateAsync(to: new PhoneNumber(number), from: new PhoneNumber(_settings.FromPhoneNumber), body: contents);

                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Verbose, "TwilioSMSSender_SendAsync", "EmailSent",
                    new System.Collections.Generic.KeyValuePair<string, string>("Subject", number),
                    new System.Collections.Generic.KeyValuePair<string, string>("to", contents));

                SendSMSMessage.Inc();

                return InvokeResult.Success;
            }
            catch (Exception ex)
            {
                _adminLogger.AddException("SendGridEmailServices_SendAsync", ex,
    new System.Collections.Generic.KeyValuePair<string, string>("number", number),
    new System.Collections.Generic.KeyValuePair<string, string>("contents", contents));

                return InvokeResult.FromException("TwilioSMSSender_SendAsync", ex);

            }
        }
    }
}
