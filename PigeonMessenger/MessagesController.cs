using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PigeonMessenger.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PigeonMessenger
{
    /// <summary>
    /// Controller layer to handle HTTP requests for Message resource.
    /// </summary>
    public class MessagesController
    {
        private readonly IDbService _snowflakeDbService;

        /// <summary>
        /// MessagesController constructor which takes in an IDbService parameter (from DI/Startup in this case).
        /// </summary>
        /// <param name="snowflakeDbService"></param>
        public MessagesController(IDbService snowflakeDbService)
        {
            _snowflakeDbService = snowflakeDbService;
        }

        /// <summary>
        /// Receives a POST request with key value pairs of 'sender' (string), 'receiver' (string), 'body' (string), and 'ispublic' (bool) in JSON body and returns the id (a guid, as a string without hyphens) of the created Message.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [FunctionName("MessagesCreate")]
        public async Task<IActionResult> MessagesCreate([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/messages")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var message = JsonSerializer.Deserialize<Message>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (String.IsNullOrEmpty(message.Sender))
            {
                return new BadRequestObjectResult("Sender is required.");
            }
            if (String.IsNullOrEmpty(message.Recipient))
            {
                return new BadRequestObjectResult("Recipient is required.");
            }
            if (String.IsNullOrEmpty(message.Body))
            {
                return new BadRequestObjectResult("Message body is required.");
            }

            // Call a data storage service to persist the provided message.
            var messageId = _snowflakeDbService.CreateMessage(message);

            return new CreatedResult($"{req.Path}/{messageId}", messageId);
        }

        /// <summary>
        /// Receives a GET request and returns Messages exchanged between the provided recipient and sender. Also takes an optional query parameter of 'since_days_ago' which
        /// can be set to an integer value with a max of 30, and which filters the result set to every message between the parties within the provided number of days.
        /// If the since_days_ago parameter is not supplied, the result set is limited to a default number that is set through a configurable environment variable (currently set to 100).
        /// </summary>
        /// <param name="req"></param>
        /// <param name="recipient"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        [FunctionName("MessagesGetForRecipientFromSpecificSender")]
        public ActionResult<List<Message>> MessagesGetForRecipientFromSpecificSender([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/messages/{recipient}/{sender}")] HttpRequest req, string recipient, string sender)
        {
            IEnumerable<Message> messages;

            if (req.Query.ContainsKey("since_days_ago"))
            {
                int sinceDaysAgo;
                try
                {
                    sinceDaysAgo = int.Parse(req.Query["since_days_ago"]) > 30 ? 30 : int.Parse(req.Query["since_days_ago"]);
                }
                catch (Exception)
                {
                    return new BadRequestObjectResult("Value provided for since_days_ago parameter could not be processed. Please provide a valid number from 0 to 30 i.e. '?since_days_ago=10'");
                }

                messages = _snowflakeDbService.GetMessagesBetweenPartiesSinceDaysAgo(recipient, sender, sinceDaysAgo);
                return new OkObjectResult(messages);
            }

            messages = _snowflakeDbService.GetMessagesBetweenPartiesWithLimit(recipient, sender, int.Parse(Environment.GetEnvironmentVariable("DefaultResultsLimit")));
            return new OkObjectResult(messages);
        }

        /// <summary>
        /// Receives a GET request and returns Messages for all senders. Also takes an optional query parameter of 'since_days_ago' which
        /// can be set to an integer value with a max of 30, and which filters the result set to every message sent within the provided number of days.
        /// If the since_days_ago parameter is not supplied, the result set is limited to a default number that is set through a configurable environment variable (currently set to 100).
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [FunctionName("MessagesGetForAllSenders")]
        public async Task<IActionResult> MessagesGetForAllSenders([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/messages")] HttpRequest req)
        {
            IEnumerable<Message> messages;

            //if (!String.IsNullOrEmpty(since_days_ago))
            if (req.Query.ContainsKey("since_days_ago"))
            {
                int sinceDaysAgo;
                try
                {
                    sinceDaysAgo = int.Parse(req.Query["since_days_ago"]) > 30 ? 30 : int.Parse(req.Query["since_days_ago"]);
                    //sinceDaysAgo = int.Parse(since_days_ago) > 30 ? 30 : int.Parse(since_days_ago);
                }
                catch (Exception)
                {
                    return new BadRequestObjectResult("Value provided for since_days_ago parameter could not be processed. Please provide a valid number from 0 to 30 i.e. '?since_days_ago=10'");
                }

                messages = _snowflakeDbService.GetMessagesAllSendersSinceDaysAgo(sinceDaysAgo);
                return new OkObjectResult(messages);
            }

            messages = _snowflakeDbService.GetMessagesAllSendersWithLimit(int.Parse(Environment.GetEnvironmentVariable("DefaultResultsLimit")));
            return new OkObjectResult(messages);
        }
    }
}
