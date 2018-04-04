using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Ai;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Linq;
using System.Threading.Tasks;
using aadv2.Topics;
using aadv2;

namespace Microsoft.Bot.Samples
{
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        BotFrameworkAdapter adapter = null;

        public MessagesController(IConfiguration configuration)
        {
            CloudStorageAccount csa = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("TABLE_CONNSTRING"));

            IStorage storage = new AzureTableStorage(csa, tableName: Environment.GetEnvironmentVariable("tablename"));

            if (adapter == null)
            {
                adapter = new BotFrameworkAdapter(Environment.GetEnvironmentVariable("MicrosoftAppId"), Environment.GetEnvironmentVariable("MicrosoftAppPassword"))
                    .Use(new ConversationStateManagerMiddleware(storage))
                    .Use(new UserStateManagerMiddleware(storage));
            }
        }

        public async Task OnReceiveActivityAsync(IBotContext context)
        {
            bool handled = false;
            var activeTopic = context.State.ConversationProperties["ActiveTopic"] as ITopic;

            if (activeTopic == null)
            {
                // use default topic
                activeTopic = new DefaultTopic();
                context.State.ConversationProperties["ActiveTopic"] = activeTopic;
                handled = await activeTopic.StartTopic(context);
            }
            else
            {
                // continue to use the active topic
                handled = await activeTopic.ContinueTopic(context);
            }

            // the bot only needs to transition from defaultTopic -> other topics and back, so 
            // if activeTopic's result is false and the activeToic is NOT the default topic, 
            // we switch back to default topic
            if (handled == false && !(context.State.ConversationProperties["ActiveTopic"] is DefaultTopic))
            {
                // resume default topic
                activeTopic = new DefaultTopic();
                context.State.ConversationProperties["ActiveTopic"] = activeTopic;
                handled = await activeTopic.ResumeTopic(context);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Activity activity)
        {
            try
            {
                await adapter.ProcessActivty(this.Request.Headers["Authorization"].FirstOrDefault(), activity, OnReceiveActivityAsync);
                return this.Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return this.Unauthorized();
            }
            catch (InvalidOperationException e)
            {
                return this.NotFound(e.Message);
            }
        }
    }
}