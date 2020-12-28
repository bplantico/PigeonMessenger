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
    public class MessagesController
    {
        private readonly SnowflakeDbService _snowflakeDbService;
        public MessagesController(SnowflakeDbService snowflakeDbService)
        {
            _snowflakeDbService = snowflakeDbService;
        }

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

        [FunctionName("MessagesGetForRecipientFromSpecificSender")]
        public async Task<IActionResult> MessagesGetForRecipientFromSpecificSender([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/messages/{recipient}/{sender}")] HttpRequest req, string recipient, string sender)
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

        [FunctionName("MessagesGetForAllSenders")]
        public async Task<IActionResult> MessagesGetForAllSenders([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/messages")] HttpRequest req)
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

                messages = _snowflakeDbService.GetMessagesAllSendersSinceDaysAgo(sinceDaysAgo);
                return new OkObjectResult(messages);
            }

            messages = _snowflakeDbService.GetMessagesAllSendersWithLimit(int.Parse(Environment.GetEnvironmentVariable("DefaultResultsLimit")));
            return new OkObjectResult(messages);
        }
    }
}
