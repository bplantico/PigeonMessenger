using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using PigeonMessenger.Contract;
using System;
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
    }
}
