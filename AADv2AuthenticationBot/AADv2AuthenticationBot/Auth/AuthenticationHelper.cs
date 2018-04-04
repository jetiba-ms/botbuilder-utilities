using Microsoft.Bot.Schema;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;

namespace aadv2.Auth
{
    public class AuthenticationHelper
    {
        /// <summary>The AuthenticationHelper is an helper to use the Microsoft Authentication Library for the user sign in.
        /// <para>MSAL makes it easy to obtain tokens from Azure AD v2 (work & school accounts, MSA) and Azure AD B2C, gaining access to Microsoft Cloud API and any other API secured by Microsoft identities.</para>
        /// <para>In this sample only Azure AD v2 authentication is implemented</para>
        /// </summary>

        public string AuthUrl { get; set; }

        public static async Task<string> GetAuthUrlAsync(ConversationReference cf)
        {
            //encode the conversation reference to pass it to the callback controller
            //it will be use to resume the context after the Sign In process
            var extraQueryParameters = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(cf)));

            //create client passing information about the registered application in Azure AD
            //registration is explained here https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-app-registration
            ConfidentialClientApplication client = new ConfidentialClientApplication(
                Environment.GetEnvironmentVariable("aad:ClientId"),
                "https://login.microsoftonline.com/" + Environment.GetEnvironmentVariable("aad:Tenant") + "/oauth2/v2.0",
                new Uri(Environment.GetEnvironmentVariable("aad:RedirectUrl")).ToString(),
                new ClientCredential(Environment.GetEnvironmentVariable("aad:ClientSecret")),
                null, null
                );

            List<string> listscopes = Environment.GetEnvironmentVariable("aad:Scopes").Split(',').ToList();

            //obtain the Authorization url
            var uri = await client.GetAuthorizationRequestUrlAsync(
                listscopes,
                null,
                $"state={extraQueryParameters}"
                );

            return uri.ToString();
        }

        public static async Task<string> GetTokenByAuthCodeAsync(string authCode)
        {
            ConfidentialClientApplication client = new ConfidentialClientApplication(
                Environment.GetEnvironmentVariable("aad:ClientId"),
                "https://login.microsoftonline.com/" + Environment.GetEnvironmentVariable("aad:Tenant") + "/oauth2/v2.0",
                new Uri(Environment.GetEnvironmentVariable("aad:RedirectUrl")).ToString(),
                new ClientCredential(Environment.GetEnvironmentVariable("aad:ClientSecret")),
                null, null
                );

            List<string> listscopes = Environment.GetEnvironmentVariable("aad:Scopes").Split(',').ToList();

            //obtain the token from the code passed by the OAuthCallback controller
            var authenticationResult = await client.AcquireTokenByAuthorizationCodeAsync(authCode, listscopes);

            return authenticationResult.AccessToken;

        }
    }
}
