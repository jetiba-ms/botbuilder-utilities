using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using aadv2.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace aadv2.Controllers
{
    public class OAuthCallbackController : Controller
    {
        /// <summary>The OAuthCallbackController is the RedirectUrl for Azure AD application.
        /// <para>Here, you receive the response from the Authorization url, with the code that you can use to retrieve authentication token</para>
        /// <para>TODO: manage token refresh</para>
        /// </summary>

        /// <summary>
        /// this method is to manage the logout
        /// at the moment during logout the bot doesn't send anything to the user
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        //[HttpGet]
        //[Route("api/OAuthCallback")]
        //public async Task<HttpResponseMessage> OAuthCallback()
        //{
        //    try
        //    {

        //        var resp = new HttpResponseMessage(HttpStatusCode.OK);
        //        resp.Content = new StringContent($"<html><body>You have been signed out. You can now close this window.</body></html>", System.Text.Encoding.UTF8, @"text/html");
        //        return resp;

        //    }
        //    catch (Exception ex)
        //    {
        //        // Callback is called with no pending message as a result the login flow cannot be resumed.
        //        return new HttpResponseMessage(HttpStatusCode.BadRequest);
        //    }

        //}

        [HttpGet]
        [Route("api/OAuthCallback")]
        public async Task<HttpResponseMessage> OAuthCallback(
            [FromQuery] string code,
            [FromQuery] string state,
            CancellationToken cancellationToken)
        {
            try
            {
                var queryParams = state;
                object tokenCache = new Microsoft.Identity.Client.TokenCache();

                var cfbytes = WebEncoders.Base64UrlDecode(queryParams);
                var _cf = JsonConvert.DeserializeObject<ConversationReference>(System.Text.Encoding.UTF8.GetString(cfbytes));

                // Exchange the Auth code with Access token
                var token = await AuthenticationHelper.GetTokenByAuthCodeAsync(code);

                // create the context from the ConversationReference
                // this require also the storage to store context.state
                CloudStorageAccount csa = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("TABLE_CONNSTRING"));
                IStorage storage = new AzureTableStorage(csa, Environment.GetEnvironmentVariable("tablename"));

                BotFrameworkAdapter bot = new BotFrameworkAdapter(Environment.GetEnvironmentVariable("MicrosoftAppId"), Environment.GetEnvironmentVariable("MicrosoftAppPassword"));
                bot.Use(new UserStateManagerMiddleware(storage));
                await bot.ContinueConversation(_cf, async (IBotContext context) =>
                {
                    // store the acces token in the BotState
                    context.State.UserProperties["token"] = token;
                    //send logged in message using context created
                    context.Reply("You are now logged in");
                });

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                // Callback is called with no pending message as a result the login flow cannot be resumed.
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }
    }
}