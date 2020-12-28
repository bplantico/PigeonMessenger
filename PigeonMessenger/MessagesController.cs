using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using PigeonMessenger.Contract;

namespace PigeonMessenger
{
    public class MessagesController
    {
        private readonly ILogger _logger;
        public MessagesController(ILogger logger)
        {
            _logger = logger;
        }

        [FunctionName("MessagesCreate")]
        public async Task<IActionResult> MessagesCreate([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/messages")] HttpRequest req)
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

            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkResult();
        }
    }
}
