using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using SendGrid.Helpers.Mail;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Services
{
    public class SendGridEmailService : IEmailSender
    {
        ILagoVistaAspNetCoreIdentityProviderSettings _settings;
        IAppConfig _appConfig;
        IAdminLogger _adminLogger;

        public SendGridEmailService(ILagoVistaAspNetCoreIdentityProviderSettings settings, IAppConfig appConfig, IAdminLogger adminLogger)
        {
            _settings = settings;
            _appConfig = appConfig;
            _adminLogger = adminLogger;
        }

        public async Task<InvokeResult> SendAsync(string email, string subject, string body)
        {

            try
            {
                body = $@"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd""><html xmlns=""http://www.w3.org/1999/xhtml""><head>
<meta name=""viewport"" content=""width=device-width, initial-scale=1, minium-scale=1, maxium-scale=1"">
	<title>NuvIoT - IoT Eneablement Platform</title>
	<style type=""text/css"">

    body
	{{
        width: 100% !important;
		-webkit-text-size-adjust: 100%;
		-ms-text-size-adjust: 100%;
		margin: 0;
		padding: 0;
	}}

.ExternalClass
	{{
        width: 100%;
	}}

#backgroundTable
		{{
            margin: 0;
			padding: 0;
			width: 100% !important;
			line-height: 100% !important;
		}}

	img
		{{
            outline: none;
			text-decoration: none;
			-ms-interpolation-mode: bicubic;
		}}

		a img
		{{
            border: none;
		}}

		.image-fix
		{{
            display: block;
		}}

			Bring inline: Yes. */
			p
			{{
                margin: 1em 0;
			}}

			/* Hotmail header color reset
			Bring inline: Yes. */
			h1, h2, h3, h4, h5
			{{
                color: #333333 !important;
				font-weight:400;
			}}

			h6
			{{
                color: #666666 !important;
			}}

				h1 a, h2 a, h3 a, h4 a, h5 a, h6 a
				{{
                    color: #2172ba !important;
				}}

					h1 a:active, h2 a:active, h3 a:active, h4 a:active, h5 a:active, h6 a:active
					{{
                        color: #2172ba !important;
					}}

					h1 a:visited, h2 a:visited, h3 a:visited, h4 a:visited, h5 a:visited, h6 a:visited
					{{
                        color: #2172ba !important;
					}}

			/* Outlook 07, 10 Padding issue fix */
			table td
			{{
                border - collapse: collapse;
			}}
            .ssrContent{{
                font - size: 12px;
            }}
			/* Remove spacing around Outlook 07, 10 tables */
			table
			{{ /*border-collapse:collapse;*/
                mso - table - lspace: 0pt;
				mso-table-rspace: 0pt;
			}}

			a
			{{
                color: #2172ba;
				text-decoration: none;
			}}

			a:hover {{
                color: #2172ba;
				text-decoration: underline;
			}}

			.ad-width{{
                width: 275px !important;
			}}
			/*.non-mobile-image, .non-mobile-email{{
                display:block !important;
			}}*/
			td[class=""mobile-email""], td[class=""mobile-image""]

            {{
                    display: none !important;
                    max - height: 0 !important;
                    width: 0 !important;
                    font - size: 0 !important;
                    line - height: 0 !important;
                    padding: 0 !important;
                    mso - hide: all; /* hide elements in Outlook 2007-2013 */
                }}
                /*div[class=""mobile-mp-card-info""] {{

                        margin:0 !important;
                        padding:90px 0 0 0 !important;                
                    }}*/
                /* Apple Mail to limit that table's size on desktop*/
                @media screen and(min - width: 600px) {{

                .container {{
                        width: 601px!important;
                    }}
                }}
                @media only screen and(max-width: 525px)
			{{
                    div[class= ""mobile - mp - card - section""] {{
                    padding:0 !important;
                    margin:-25px 0 0 -150px !important;
                }}
}}
/* More Specific Targeting */
/* MOBILE TARGETING 480px */
@media only screen and(min-width: 480px)
{{

    /*table[class=""table""], td[class=""cell""]
    {{
        width: 304px !important;
    }}*/

    td[class= ""mobile - ad - spacing""]
			{{
				padding-bottom: 15px !important;
			}}
                td[class=""mobile-image""]
			{{
				display: inline-block !important;
			}}
             
			td[class=""non-mobile-image""] /*, .non-mobile-email*/
			{{
				display: none !important;
				max-height: 0 !important;
				width: 0 !important;
				font-size: 0 !important;
				line-height: 0 !important;
				padding: 0 !important;
				/*mso-hide: all;  hide elements in Outlook 2007-2013 */
			}}
			/*.mobile-email{{
				display:block !important;
			}}*/
			tr[class=""mobile-footer""]
			{{
				background-color: #ffffff !important;
			}}

				body[class=""mobile-body""]
				{{
					background-color: #ffffff !important;
				}}
				div[class=""navigation""] {{
					width:100% !important;
				}}
				div[class=""navigation""] a {{
					display:block !important;
					text-decoration:none !important;
					text-align:center !important;
					font-size:16px !important;
				}}
				/*Controlling phone number linking for mobile. */
				a[href ^= ""tel""], a[href ^= ""sms""]
				{{
					text-decoration: none;
					color: #2172ba;
					pointer-events: none;
					cursor: default;
				}}

				.mobile_link a[href ^= ""telephone""], .mobile_link a[href ^= ""sms""]
				{{
					text-decoration: none;
					color: #2172ba !important;
					pointer-events: auto;
					cursor: default;
				}}

				a[class=""mobile-show""], a[class=""mobile-hide""]
				{{
					display: block !important;
					color: #2172ba !important;
					border-radius: 20px;
					padding: 0 8px;
					text-decoration: none;
					font-weight: bold;
					font-family: ""Helvetica Neue"", Helvetica, sans-serif;
					font-size: 11px;
					position: absolute;
					top: 25px;
					right: 10px;
					text-align: center;
					width: 40px;
				}}
				div[class=""ad-width""]{{
					width:300px !important;
				}}
            }}
			@media only screen and(min-device-width: 768px) and(max-device-width: 1024px)
{{
    /*ipad (tablets, smaller screens, etc) */
    a[href ^= ""tel""],a[href ^= ""sms""]

                {{
        text-decoration: none;
        color: #2172ba;
					pointer-events: none;
        cursor: default;
    }}

				.mobile_link a[href ^= ""tel""], .mobile_link a[href ^= ""sms""]
                {{
        text - decoration: default;
        color: #2172ba !important;
					pointer - events: auto;
        cursor: default;
    }}
}}

@media only screen and(-webkit-min-device-pixel-ratio: 2)
{{
    /*iPhone 4g styles */
    tr[class= ""non-mobile-image""],img[class= ""non - mobile - image""], table[class=""non-mobile-email""]
				{{
					display: none !important;
					max-height: 0;
					width: 0;
					font-size: 0;
					line-height: 0;
					padding: 0;
					mso-hide: all; /* hide elements in Outlook 2007-2013 */
				}}

    </style>

<body style=""padding: 0; margin: 0; -webkit-text-size-adjust: none; -ms-text-size-adjust: 100%; background-color: #e6e6e6;"" class=""mobile-body"">
		<!--[if (gte mso 9)|(IE)]>
                
		<![endif]-->
		<table width=""100%"" cellpadding=""20"" cellspacing=""0"" border=""0"" style=""max-width:600px; background-color:white""><div>
            <tr>
                <td>
                    <img src=""{_appConfig.AppLogo}"" />
                    <h1>{_appConfig.AppName}</h1>
                    <div style=""width:550px"">
                    {body}
                    </div>
                </td>
            </tr>
        </table>
    <p>Please do not reply to this email as it is an unmonitored address.</p>
    <a href=""mailto:support@software-logistics.com"">Contact Support</a>

</div>
</body>
</html>";


				var msg = new SendGridMessage();
				msg.AddTo(email);
				msg.From = new EmailAddress(_settings.SmtpFrom, "NuvIoT Notifications") ;
				msg.Subject = subject;
				msg.AddContent(MediaTypeNames.Text.Html, body);

				if(String.IsNullOrEmpty(_settings.SmtpServer.Password))
                {
					throw new ArgumentNullException("SMTP Server API Key (SendGrid) is null or empty");
                }

				var client = new SendGrid.SendGridClient(_settings.SmtpServer.Password);
				var response = await client.SendEmailAsync(msg);

                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Verbose, "SendGridEmailServices_SendAsync", "EmailSent",
                    new System.Collections.Generic.KeyValuePair<string, string>("Subject", subject),
                    new System.Collections.Generic.KeyValuePair<string, string>("to", email));

                return InvokeResult.Success;
            }
            catch (Exception ex)
            {
                _adminLogger.AddException("SendGridEmailServices_SendAsync", ex,
                    new System.Collections.Generic.KeyValuePair<string, string>("Subject", subject),
                    new System.Collections.Generic.KeyValuePair<string, string>("to", email));

                return InvokeResult.FromException("SendGridEmailServices_SendAsync", ex);
            }

        }
    }
}
