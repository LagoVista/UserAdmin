using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Interfaces;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Models.Security;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Services
{
    public class TwitterAuthServices : ITwitterAuthService
    {
        private readonly IOAuthSettings _oauthSettings;
        private readonly IAppConfig _appConfig;
        private readonly IAdminLogger _adminLogger;

        const string REQUEST_TOKEN_ENDPOINT = "https://api.twitter.com/oauth/request_token";
        const string ACCESS_TOKEN_ENDPOINT = "https://api.twitter.com/oauth/access_token";

        public TwitterAuthServices(IOAuthSettings oauthSettings, IAppConfig appConfig, IAdminLogger adminLogger)
        {
            _oauthSettings = oauthSettings ?? throw new ArgumentNullException(nameof(oauthSettings));
            _appConfig = appConfig;
            _adminLogger = adminLogger;
        }

        private async Task<HttpResponseMessage> ExecuteRequestAsync(string url,
                        HttpMethod httpMethod, TwitterRequestToken accessToken = null,
                        Dictionary<string, string> extraOAuthPairs = null,
                        Dictionary<string, string> queryParameters = null, Dictionary<string, string> formData = null,
                        CancellationToken? cancellationToken = null)
        {
            var authorizationParts = new SortedDictionary<string, string>(extraOAuthPairs ?? new Dictionary<string, string>())
            {
                { "oauth_consumer_key", _oauthSettings.TwitterOAuth.ClientId },
                { "oauth_nonce", Guid.NewGuid().ToString("N") },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", GenerateTimeStamp() },
                { "oauth_version", "1.0" }
            };

            if (accessToken != null)
            {
                authorizationParts.Add("oauth_token", accessToken.Token);
            }

            var signatureParts = new SortedDictionary<string, string>(authorizationParts);
            if (queryParameters != null)
            {
                foreach (var queryParameter in queryParameters)
                {
                    signatureParts.Add(queryParameter.Key, queryParameter.Value);
                }
            }
            if (formData != null)
            {
                foreach (var formItem in formData)
                {
                    signatureParts.Add(formItem.Key, formItem.Value);
                }
            }

            var parameterBuilder = new StringBuilder();
            foreach (var signaturePart in signatureParts)
            {
                parameterBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}&", Uri.EscapeDataString(signaturePart.Key), Uri.EscapeDataString(signaturePart.Value));
            }
            parameterBuilder.Length--;
            var parameterString = parameterBuilder.ToString();

            var canonicalizedRequestBuilder = new StringBuilder();
            canonicalizedRequestBuilder.Append(httpMethod.Method);
            canonicalizedRequestBuilder.Append('&');
            canonicalizedRequestBuilder.Append(Uri.EscapeDataString(url));
            canonicalizedRequestBuilder.Append('&');
            canonicalizedRequestBuilder.Append(Uri.EscapeDataString(parameterString));

            _adminLogger.Trace($"[AuthTOkenManager__ExecuteRequestAsync] OAUth Statement {canonicalizedRequestBuilder}");

            foreach(var hdr in authorizationParts)
            {
                _adminLogger.Trace($"\t\t[AuthTOkenManager__ExecuteRequestAsync] hdr: {hdr.Key}={hdr.Value}");
            }

            var signature = ComputeSignature(_oauthSettings.TwitterOAuth.Secret, accessToken?.TokenSecret, canonicalizedRequestBuilder.ToString());
            authorizationParts.Add("oauth_signature", signature);

            var queryString = "";
            if (queryParameters != null)
            {
                var queryStringBuilder = new StringBuilder("?");
                foreach (var queryParam in queryParameters)
                {
                    queryStringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}&", queryParam.Key, queryParam.Value);
                }
                queryStringBuilder.Length--;
                queryString = queryStringBuilder.ToString();
            }

            var authorizationHeaderBuilder = new StringBuilder();
            authorizationHeaderBuilder.Append("OAuth ");
            foreach (var authorizationPart in authorizationParts)
            {
                authorizationHeaderBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}=\"{1}\",", authorizationPart.Key, Uri.EscapeDataString(authorizationPart.Value));
            }
            authorizationHeaderBuilder.Length--;


            var request = new HttpRequestMessage(httpMethod, url + queryString);
            request.Headers.Add("Authorization", authorizationHeaderBuilder.ToString());

            _adminLogger.Trace($"[AuthTOkenManager__ExecuteRequestAsync] Request To: {request.RequestUri}");

            // This header is so that the error response is also JSON - without it the success response is already JSON
            request.Headers.Add("Accept", "application/json");

            if (formData != null)
            {
                request.Content = new FormUrlEncodedContent(formData!);
            }

            var client = new HttpClient();
            return await client.SendAsync(request, cancellationToken ?? CancellationToken.None);
        }

        public async Task<TwitterRequestToken> ObtainRequestTokenAsync(CancellationToken? token = null)
        {
            var callBackUri = $"{_appConfig.WebAddress}/account/oauthtwitter/authorize/callback";

            _adminLogger.Trace($"[AuthTOkenManager__ObtainRequestTokenAsync] Callback URI:  {callBackUri}");

            var response = await ExecuteRequestAsync(REQUEST_TOKEN_ENDPOINT, HttpMethod.Post, 
                  extraOAuthPairs: new Dictionary<string, string> { {  "oauth_callback", callBackUri } });

            await EnsureTwitterRequestSuccess(response);

            var responseText = await response.Content.ReadAsStringAsync(token ?? CancellationToken.None);
            var responseParameters = new FormCollection(new FormReader(responseText).ReadForm());
            if (!string.Equals(responseParameters["oauth_callback_confirmed"], "true", StringComparison.Ordinal))
            {
                throw new Exception("Twitter oauth_callback_confirmed is not true.");
            }

            return new TwitterRequestToken
            {
                Token = Uri.UnescapeDataString(responseParameters["oauth_token"].ToString()),
                TokenSecret = Uri.UnescapeDataString(responseParameters["oauth_token_secret"].ToString()),
                CallbackConfirmed = true,
            };
        }

        public async Task<TwitterAccessToken> ObtainAccessTokenAsync(TwitterRequestToken token, string verifier, CancellationToken? cancellationToken = null)
        {
            // https://developer.twitter.com/en/docs/authentication/api-reference/access_token


            var formPost = new Dictionary<string, string> { { "oauth_verifier", verifier } };
            var response = await ExecuteRequestAsync(ACCESS_TOKEN_ENDPOINT, HttpMethod.Post, token, formData: formPost);

            if (!response.IsSuccessStatusCode)
            {
                await EnsureTwitterRequestSuccess(response); // throw
            }

            var responseText = await response.Content.ReadAsStringAsync(cancellationToken ?? CancellationToken.None);
            var responseParameters = new FormCollection(new FormReader(responseText).ReadForm());

            return new TwitterAccessToken
            {
                Token = Uri.UnescapeDataString(responseParameters["oauth_token"].ToString()),
                TokenSecret = Uri.UnescapeDataString(responseParameters["oauth_token_secret"].ToString()),
                UserId = Uri.UnescapeDataString(responseParameters["user_id"].ToString()),
                ScreenName = Uri.UnescapeDataString(responseParameters["screen_name"].ToString()),
            };
        }
        private string GenerateTimeStamp()
        {
            var secondsSinceUnixEpocStart = DateTime.UtcNow - DateTimeOffset.UnixEpoch;
            return Convert.ToInt64(secondsSinceUnixEpocStart.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        private static string ComputeSignature(string consumerSecret, string tokenSecret, string signatureData)
        {
            var key = Encoding.ASCII.GetBytes(
                string.Format(CultureInfo.InvariantCulture,
                    "{0}&{1}",
                    Uri.EscapeDataString(consumerSecret),
                    string.IsNullOrEmpty(tokenSecret) ? string.Empty : Uri.EscapeDataString(tokenSecret)));

            HMACSHA1 myhmacsha1 = new HMACSHA1(key);
            byte[] byteArray = Encoding.ASCII.GetBytes(signatureData);
            using (var stream = new MemoryStream(byteArray))
            {
                var hash = myhmacsha1.ComputeHash(stream);
                return  Convert.ToBase64String(hash);
            }
        }

        // https://developer.twitter.com/en/docs/apps/callback-urls
        private async Task EnsureTwitterRequestSuccess(HttpResponseMessage response, CancellationToken? cancellation = null)
        {
            var contentTypeIsJson = string.Equals(response.Content.Headers.ContentType?.MediaType ?? "", "application/json", StringComparison.OrdinalIgnoreCase);
            if (response.IsSuccessStatusCode || !contentTypeIsJson)
            {
                // Not an error or not JSON, ensure success as usual
                response.EnsureSuccessStatusCode();
                return;
            }

            TwitterErrorResponse errorResponse;
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Failure, attempt to parse Twitters error message
                var errorContentStream = await response.Content.ReadAsStreamAsync(cancellation ?? CancellationToken.None);
                errorResponse = await JsonSerializer.DeserializeAsync<TwitterErrorResponse>(errorContentStream, options);
            }
            catch
            {
                // No valid Twitter error response, throw as normal
                response.EnsureSuccessStatusCode();
                return;
            }

            if (errorResponse == null)
            {
                // No error message body
                response.EnsureSuccessStatusCode();
                return;
            }

            var errorMessageStringBuilder = new StringBuilder("An error has occurred while calling the Twitter API, error's returned:");

            if (errorResponse.Errors != null)
            {
                foreach (var error in errorResponse.Errors)
                {
                    errorMessageStringBuilder.Append(Environment.NewLine);
                    errorMessageStringBuilder.Append(FormattableString.Invariant($"Code: {error.Code}, Message: '{error.Message}'"));
                }
            }

            throw new InvalidOperationException(errorMessageStringBuilder.ToString());
        }

    }
}
