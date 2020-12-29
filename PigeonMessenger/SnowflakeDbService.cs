using Microsoft.Extensions.Logging;
using PigeonMessenger.Contract;
using Snowflake.Data.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.Json;

namespace PigeonMessenger
{
    /// <summary>
    /// Service class to interact with a Snowflake database.
    /// </summary>
    public class SnowflakeDbService : IDbService
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor which takes an ILogger, in this case from DI/startup.
        /// </summary>
        /// <param name="logger"></param>
        public SnowflakeDbService(ILogger<SnowflakeDbService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Takes a Message as a parameter and inserts the Message into the messages table using a parameterized query to guard against SQL injection attacks.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string CreateMessage(Message message)
        {
            var cleanSender = message.Sender.Trim().ToLower();
            var cleanRecipient = message.Recipient.Trim().ToLower();
            message.Id = Guid.NewGuid().ToString("N");

            using var conn = new SnowflakeDbConnection();

            try
            {
                var connectionString = $"account={Environment.GetEnvironmentVariable("SnowflakeAccount")};" +
                                        $"user={Environment.GetEnvironmentVariable("SnowflakeUser")};" +
                                        $"password={Environment.GetEnvironmentVariable("SnowflakePassword")};" +
                                        $"db={Environment.GetEnvironmentVariable("SnowflakeDb")};" +
                                        $"schema={Environment.GetEnvironmentVariable("SnowflakeSchema")}";

                conn.ConnectionString = connectionString;
                conn.Open();

                using var cmd = conn.CreateCommand();

                cmd.CommandText = "INSERT INTO messages (id, sender, recipient, body, isPublic) VALUES (?,?,?,?,?)";

                var p1 = cmd.CreateParameter();
                p1.ParameterName = "1";
                p1.Value = message.Id;
                p1.DbType = DbType.String;
                cmd.Parameters.Add(p1);

                var p2 = cmd.CreateParameter();
                p2.ParameterName = "2";
                p2.Value = cleanSender;
                p2.DbType = DbType.String;
                cmd.Parameters.Add(p2);

                var p3 = cmd.CreateParameter();
                p3.ParameterName = "3";
                p3.Value = cleanRecipient;
                p3.DbType = DbType.String;
                cmd.Parameters.Add(p3);

                var p4 = cmd.CreateParameter();
                p4.ParameterName = "4";
                p4.Value = message.Body;
                p4.DbType = DbType.String;
                cmd.Parameters.Add(p4);

                var p5 = cmd.CreateParameter();
                p5.ParameterName = "5";
                p5.Value = message.IsPublic;
                p5.DbType = DbType.Boolean;
                cmd.Parameters.Add(p5);

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

            return message.Id;
        }

        /// <summary>
        /// Takes 'recipient', 'sender', and 'sinceDaysAgo' parameters and queries the messages table for all messages between the provided parties during the number of days provided, up to a maximum of 30 days.
        /// Results are returned in descending order by the time they were created at.
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="sender"></param>
        /// <param name="sinceDaysAgo"></param>
        /// <returns></returns>
        public IEnumerable<Message> GetMessagesBetweenPartiesSinceDaysAgo(string recipient, string sender, int sinceDaysAgo)
        {
            var cleanRecipient = recipient.Trim().ToLower();
            var cleanSender = sender.Trim().ToLower();
            var startDateTimeFilter = DateTime.UtcNow.AddDays(-sinceDaysAgo).ToString("yyyy-MM-dd HH:mm:ss");
            var messages = new List<Message>();

            // parameterized to prevent SQL injection and to handle correctly parsing special characters.
            var parameterizedSql = $"SELECT * " +
                               $"FROM messages " +
                               $"WHERE createdAt > (?) " + // startDateTimeFilter
                               $"AND isPublic = true " +
                               $"AND (recipient = (?) AND sender = (?)) " + // cleanRecipient, cleanSender
                               $"OR (recipient = (?) AND sender = (?)) " + // cleanSender, cleanRecipient
                               $"ORDER BY createdAt DESC;"; // To-do: May consider adding a limit to this call i.e. what if 10,000 messages were exchanged between these two parties in the time provided? How useful would that result set be?
            
            var parameters = new List<SnowflakeDbParameter>();
            var p1 = new SnowflakeDbParameter();
            p1.ParameterName = "1"; // Don't change (unless query changes, of course). These correspond to the position they'll take in the query string.
            p1.Value = startDateTimeFilter;
            p1.DbType = DbType.String;
            parameters.Add(p1);

            var p2 = new SnowflakeDbParameter();
            p2.ParameterName = "2";
            p2.Value = cleanRecipient;
            p2.DbType = DbType.String;
            parameters.Add(p2);

            var p3 = new SnowflakeDbParameter();
            p3.ParameterName = "3";
            p3.Value = cleanSender;
            p3.DbType = DbType.String;
            parameters.Add(p3);

            var p4 = new SnowflakeDbParameter();
            p4.ParameterName = "4";
            p4.Value = cleanSender;
            p4.DbType = DbType.String;
            parameters.Add(p4);

            var p5 = new SnowflakeDbParameter();
            p5.ParameterName = "5";
            p5.Value = cleanRecipient;
            p5.DbType = DbType.String;
            parameters.Add(p5);

            ExecuteGetMessagesQuery(messages, parameterizedSql, parameters);

            return messages;
        }

        /// <summary>
        /// Takes a 'limit' parameter and queries the messages tabel for all messages from any/all senders, up to the provided limit.
        /// Results are returned in descending order by the time they were created at.
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public IEnumerable<Message> GetMessagesAllSendersWithLimit(int limit)
        {
            var messages = new List<Message>();

            var sqlStatement = $"SELECT * " +
                               $"FROM messages " +
                               $"WHERE isPublic = true " +
                               $"ORDER BY createdAt DESC " +
                               $"LIMIT {limit};";

            var parameters = new List<SnowflakeDbParameter>();

            ExecuteGetMessagesQuery(messages, sqlStatement, parameters);

            return messages;
        }

        /// <summary>
        /// Takes a 'sinceDaysAgo' parameter and queries the messages table for all messages from any/all senders during the number of days provided, up to a maximum of 30 days.
        /// Results are returned in descending order by the time they were created at.
        /// </summary>
        /// <param name="sinceDaysAgo"></param>
        /// <returns></returns>
        public IEnumerable<Message> GetMessagesAllSendersSinceDaysAgo(int sinceDaysAgo)
        {
            var startDateTimeFilter = DateTime.UtcNow.AddDays(-sinceDaysAgo).ToString("yyyy-MM-dd HH:mm:ss");
            var messages = new List<Message>();

            var sqlStatement = $"SELECT * " +
                               $"FROM messages " +
                               $"WHERE createdAt > '{startDateTimeFilter}' " +
                               $"AND isPublic = true " +
                               $"ORDER BY createdAt DESC;";
            
            var parameters = new List<SnowflakeDbParameter>();

            ExecuteGetMessagesQuery(messages, sqlStatement, parameters);

            return messages;
        }

        /// <summary>
        /// Takes 'recipient', 'sender', and 'sinceDaysAgo' parameters and queries the messages table for all messages between the provided parties, up to the provided limit.
        /// Results are returned in descending order by the time they were created at.
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="sender"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public IEnumerable<Message> GetMessagesBetweenPartiesWithLimit(string recipient, string sender, int limit)
        {
            var cleanRecipient = recipient.Trim().ToLower();
            var cleanSender = sender.Trim().ToLower();
            
            var messages = new List<Message>();
            
            var sqlStatement = $"SELECT * " +
                               $"FROM messages " +
                               $"WHERE isPublic = true " +
                               $"AND (recipient = (?) AND sender = (?)) " + // cleanRecipient, cleanSender
                               $"OR (recipient = (?) AND sender = (?)) " + // cleanSender, cleanRecipient
                               $"ORDER BY createdAt DESC " +
                               $"LIMIT {limit};";

            var parameters = new List<SnowflakeDbParameter>();
            var p1 = new SnowflakeDbParameter();
            p1.ParameterName = "1";
            p1.Value = cleanRecipient;
            p1.DbType = DbType.String;
            parameters.Add(p1);

            var p2 = new SnowflakeDbParameter();
            p2.ParameterName = "2";
            p2.Value = cleanSender;
            p2.DbType = DbType.String;
            parameters.Add(p2);

            var p3 = new SnowflakeDbParameter();
            p3.ParameterName = "3";
            p3.Value = cleanSender;
            p3.DbType = DbType.String;
            parameters.Add(p3);

            var p4 = new SnowflakeDbParameter();
            p4.ParameterName = "4";
            p4.Value = cleanRecipient;
            p4.DbType = DbType.String;
            parameters.Add(p4);

            ExecuteGetMessagesQuery(messages, sqlStatement, parameters);

            return messages;
        }


        private void ExecuteGetMessagesQuery(List<Message> messages, string sqlStatement, List<SnowflakeDbParameter> parameters)
        {
            using var conn = new SnowflakeDbConnection();
            try
            {
                var connectionString = $"account={Environment.GetEnvironmentVariable("SnowflakeAccount")};" +
                                        $"user={Environment.GetEnvironmentVariable("SnowflakeUser")};" +
                                        $"password={Environment.GetEnvironmentVariable("SnowflakePassword")};" +
                                        $"db={Environment.GetEnvironmentVariable("SnowflakeDb")};" +
                                        $"schema={Environment.GetEnvironmentVariable("SnowflakeSchema")}";

                conn.ConnectionString = connectionString;
                conn.Open();

                using var cmd = conn.CreateCommand();

                cmd.CommandText = sqlStatement;

                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }

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
    }
}
