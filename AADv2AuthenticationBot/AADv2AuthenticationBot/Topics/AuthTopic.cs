using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using aadv2.Auth;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace aadv2.Topics
{
    internal class AuthTopic : ITopic
    {
        public string Name { get; set; } = "Auth";

        public Task<bool> StartTopic(IBotContext context)
        {
            return ContinueTopic(context);
        }

        private async Task<bool> PromptForLogin(IBotContext context)
        {
            if(context.Request.AsMessageActivity().Text == "signin")
            {
                //show card for the LogIn 
                IMessageActivity activity = ((Activity)context.Request).CreateReply();
                var OAuthSignInUrl = await AuthenticationHelper.GetAuthUrlAsync(context.ConversationReference);

                var card = new SigninCard(
                    text: "Sign in",
                    buttons: new CardAction[] {
                    new CardAction(
                        type: "signin",
                        title: "Sign In",
                        text: "Sign In",
                        value: $"{OAuthSignInUrl}"
                        )
                    });

                activity.Attachments.Add(new Attachment(HeroCard.ContentType, content: card));

                context.Reply(activity);
                //if you want to send the link as a hyperlink --> context.Reply($"[Click Here to Sign In]({OAuthSignInUrl})");
                return true;
            }
            else if(context.Request.AsMessageActivity().Text == "logout")
            {
                context.State.UserProperties.Remove("token");
                string signoutURl = "https://login.microsoftonline.com/common/oauth2/logout";
                context.Reply($"In order to finish the sign out, please click at this [link]({signoutURl}).");
                return true;
            }
            return false;
        }

        public async Task<bool> ContinueTopic(IBotContext context)
        {
            if (context.Request.Type == ActivityTypes.Message)
            {
                return await PromptForLogin(context);
            }
            return false;
        }

        public async Task<bool> ResumeTopic(IBotContext context)
        {
            return await PromptForLogin(context);
        }
    }
}