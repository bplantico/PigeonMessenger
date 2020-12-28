using Microsoft.Extensions.Logging;
using PigeonMessenger.Contract;
using Snowflake.Data.Client;
using System;
using System.Collections.Generic;
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

        internal IEnumerable<Message> GetMessagesBetweenPartiesSinceDaysAgo(string recipient, string sender, int sinceDaysAgo)
        {
            var startDateTimeFilter = DateTime.UtcNow.AddDays(-sinceDaysAgo).ToString("yyyy-MM-dd HH:mm:ss");
            var messages = new List<Message>();

            var sqlStatement = $"SELECT * " +
                               $"FROM messages " +
                               $"WHERE recipient = '{recipient}' " +
                               $"AND sender = '{sender}' " +
                               $"AND updatedAt > '{startDateTimeFilter}' " + // Using updatedAt instead of createdAt since if a message is edited, that's the intended message (might refactor to make an edited message a new message?)
                               $"AND isPublic = true " +
                               $"ORDER BY updatedAt DESC;"; 

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
                    IDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var record = (IDataRecord)reader;
                        var message = new Message();
                        message.Id = $"{record["ID"]}";
                        message.Sender = $"{record["SENDER"]}";
                        message.Recipient = $"{record["RECIPIENT"]}";
                        message.Body = $"{record["BODY"]}";
                        message.CreatedAt = DateTime.Parse($"{record["CREATEDAT"]}");
                        message.UpdatedAt = DateTime.Parse($"{record["UPDATEDAT"]}");

                        messages.Add(message);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to execute Snowflake query: {e.Message}");
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }

            return messages;
        }

        internal IEnumerable<Message> GetMessagesAllSendersWithLimit(int limit)
        {
            var messages = new List<Message>();

            var sqlStatement = $"SELECT * " +
                               $"FROM messages " +
                               $"WHERE isPublic = true " +
                               $"ORDER BY updatedAt DESC " +
                               $"LIMIT {limit};";

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
                    IDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var record = (IDataRecord)reader;
                        var message = new Message();
                        message.Id = $"{record["ID"]}";
                        message.Sender = $"{record["SENDER"]}";
                        message.Recipient = $"{record["RECIPIENT"]}";
                        message.Body = $"{record["BODY"]}";
                        message.CreatedAt = DateTime.Parse($"{record["CREATEDAT"]}");
                        message.UpdatedAt = DateTime.Parse($"{record["UPDATEDAT"]}");

                        messages.Add(message);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to execute Snowflake query: {e.Message}");
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }

            return messages;
        }

        internal IEnumerable<Message> GetMessagesAllSendersSinceDaysAgo(int sinceDaysAgo)
        {
            var startDateTimeFilter = DateTime.UtcNow.AddDays(-sinceDaysAgo).ToString("yyyy-MM-dd HH:mm:ss");
            var messages = new List<Message>();

            var sqlStatement = $"SELECT * " +
                               $"FROM messages " +
                               $"WHERE updatedAt > '{startDateTimeFilter}' " + // Using updatedAt instead of createdAt since if a message is edited, that's the intended message (might refactor to make an edited message a new message?)
                               $"AND isPublic = true " +
                               $"ORDER BY updatedAt DESC;";

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
                    IDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var record = (IDataRecord)reader;
                        var message = new Message();
                        message.Id = $"{record["ID"]}";
                        message.Sender = $"{record["SENDER"]}";
                        message.Recipient = $"{record["RECIPIENT"]}";
                        message.Body = $"{record["BODY"]}";
                        message.CreatedAt = DateTime.Parse($"{record["CREATEDAT"]}");
                        message.UpdatedAt = DateTime.Parse($"{record["UPDATEDAT"]}");

                        messages.Add(message);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to execute Snowflake query: {e.Message}");
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }

            return messages;
        }

        internal IEnumerable<Message> GetMessagesBetweenPartiesWithLimit(string recipient, string sender, int limit)
        {
            var messages = new List<Message>();

            var sqlStatement = $"SELECT * " +
                               $"FROM messages " +
                               $"WHERE recipient = '{recipient}' " +
                               $"AND sender = '{sender}' " +
                               $"AND isPublic = true " +
                               $"ORDER BY updatedAt DESC " +
                               $"LIMIT {limit};";

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
                    IDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var record = (IDataRecord)reader;
                        var message = new Message();
                        message.Id = $"{record["ID"]}";
                        message.Sender = $"{record["SENDER"]}";
                        message.Recipient = $"{record["RECIPIENT"]}";
                        message.Body = $"{record["BODY"]}";
                        message.CreatedAt = DateTime.Parse($"{record["CREATEDAT"]}");
                        message.UpdatedAt = DateTime.Parse($"{record["UPDATEDAT"]}");

                        messages.Add(message);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to execute Snowflake query: {e.Message}");
                    throw;
                }
                finally
                {
                    conn.Close();
                }
            }

            return messages;
        }
    }
}
