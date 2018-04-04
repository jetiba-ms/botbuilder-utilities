using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace aadv2.Topics
{
    public class DefaultTopic : ITopic
    {
        public DefaultTopic() { }

        public string Name { get; set; } = "Default";

        public Task<bool> StartTopic(IBotContext context)
        {
            switch (context.Request.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    //do something as greeting the user...
                    break;
                case ActivityTypes.Message:
                    return this.ContinueTopic(context);
            }
            return Task.FromResult(true);
        }

        public async Task<bool> ContinueTopic(IBotContext context)
        {
            var activeTopic = (ITopic)context.State.ConversationProperties["ActiveTopic"];

            switch (context.Request.Type)
            {
                case ActivityTypes.Message:
                    var userinfo = new JObject();
                    var user = "";
                    //check if the user is authenticated
                    if (context.State.UserProperties["token"] != null)
                    {
                        if (context.Request.AsMessageActivity().Text == "logout")
                        {
                            activeTopic = new AuthTopic();
                            context.State.ConversationProperties["ActiveTopic"] = activeTopic;
                            return await activeTopic.StartTopic(context);
                        }
                        else
                        {
                            try
                            {
                                string graphRequest = String.Format(CultureInfo.InvariantCulture, "https://graph.microsoft.com/v1.0/me");
                                HttpClient client = new HttpClient();
                                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, graphRequest);
                                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.State.UserProperties["token"]);
                                HttpResponseMessage response = await client.SendAsync(request);
                                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    string content = await response.Content.ReadAsStringAsync();
                                    userinfo = JObject.Parse(content);
                                    user = userinfo["displayName"].ToString();
                                    context.Reply(user + " said: " + context.Request.AsMessageActivity().Text);
                                    return await Task.FromResult(true);
                                }
                                else
                                {
                                    activeTopic = new AuthTopic();
                                    context.State.ConversationProperties["ActiveTopic"] = activeTopic;
                                    context.Request.AsMessageActivity().Text = "signin";
                                    return await activeTopic.StartTopic(context);
                                }

                            }
                            catch (Exception e)
                            {

                            }
                        }
                        
                    }
                    else
                    {
                        if(context.Request.AsMessageActivity().Text == "logout")
                        {
                            context.Reply("You are not logged in");
                            return await Task.FromResult(true);
                        }
                        else
                        {
                            activeTopic = new AuthTopic();
                            context.State.ConversationProperties["ActiveTopic"] = activeTopic;
                            context.Request.AsMessageActivity().Text = "signin";
                            return await activeTopic.StartTopic(context);
                        }
                    }
                    return await Task.FromResult(false);
                default:
                    return await Task.FromResult(false);

            }

        }

        public Task<bool> ResumeTopic(IBotContext context)
        {
            // just prompt the user to ask what they want to do
            //context.Reply("What can I do for you?");
            return ContinueTopic(context);
        }


    }
}
