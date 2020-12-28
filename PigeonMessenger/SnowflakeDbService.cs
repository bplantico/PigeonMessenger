using Microsoft.Extensions.Logging;
using PigeonMessenger.Contract;
using Snowflake.Data.Client;
using System;
using System.Data;
using System.Text.Json;

namespace PigeonMessenger
{
    public class SnowflakeDbService
    {
        private readonly ILogger _logger;
        public SnowflakeDbService(ILogger<SnowflakeDbService> logger)
        {
            _logger = logger;
        }

        internal string CreateMessage(Message message)
        {
            var cleanSender = message.Sender.Trim().ToLower();
            var cleanRecipient = message.Recipient.Trim().ToLower();
            message.Id = Guid.NewGuid().ToString("N");
            var serializedBody = JsonSerializer.Serialize(message.Body);

            // May need this to create timestamp TO_TIMESTAMP ('{message.CreatedAt.ToString("yyyy-MM-dd HH:m:ss")}', 'YYYY-MM-DD HH24:MI:SS')
            var sqlStatement = $"INSERT INTO messages (id, sender, recipient, body, isPublic) " +
                               $"SELECT '{message.Id}', '{cleanSender}', '{cleanRecipient}', '{serializedBody}', {message.IsPublic};";

            using (IDbConnection conn = new SnowflakeDbConnection())
            {
                try
                {
                    var connectionString = $"account={Environment.GetEnvironmentVariable("SnowflakeAccount")};" +
                                           $"user={Environment.GetEnvironmentVariable("SnowflakeUser")};" +
                                           $"password={Environment.GetEnvironmentVariable("SnowflakePassword")};" +
                                           $"db={Environment.GetEnvironmentVariable("SnowflakeDb")};" +
                                           $"schema={Environment.GetEnvironmentVariable("SnowflakeSchema")}";

                    conn.ConnectionString = connectionString;
                    conn.Open();

                    IDbCommand cmd = conn.CreateCommand();

                    cmd.CommandText = sqlStatement;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to add new record to Snowflake: {e.Message}");
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }

            return message.Id;
        }
    }
}
