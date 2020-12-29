using PigeonMessenger.Contract;
using System;
using Xunit;

namespace PigeonMessenger.Tests
{
    public class SnowflakeDbServiceTests
    {
        private readonly IDbService _snowflakeDbService;

        public SnowflakeDbServiceTests(IDbService snowflakeDbService)
        {
            _snowflakeDbService = snowflakeDbService;
            Environment.SetEnvironmentVariable("SnowflakeAccount", "ic91294.west-us-2.azure");
            Environment.SetEnvironmentVariable("SnowflakeUser", "BPLANTICO");
            Environment.SetEnvironmentVariable("SnowflakePassword", "Snowflake33!");
            Environment.SetEnvironmentVariable("SnowflakeDb", "GUILD_TECH_CHALLENGE");
            Environment.SetEnvironmentVariable("SnowflakeSchema", "TEST");
        }

        [Fact]
        public void CreateMessage_GivenValidInput_InsertsRecordAndReturnsId()
        {
            // Setup
            var message = new Message
            {
                Sender = "Tom",
                Recipient = "Jerry",
                Body = "Please meet me in the kitchen at noon.",
                IsPublic = true
            };

            // Exercise
            var newMessageId = _snowflakeDbService.CreateMessage(message);
            
            // Assert
            Assert.NotNull(newMessageId);
            Assert.Equal(32, newMessageId.Length);

            // Teardown
            _snowflakeDbService.DeleteMessage(newMessageId);
        }

        [Fact]
        public void GetMessagesBetweenPartiesSinceDaysAgo_ReturnsLimitedNumberOfMessagesBetweenTwoParties()
        {
            // Test database preloaded with four messages between tom and jerry, along with one message from spike_bulldog to tom.
            
            var messages = _snowflakeDbService.GetMessagesBetweenPartiesWithLimit("tom", "jerry", 3);

            // Do we get the correct number of messages back?
            Assert.Equal(3, messages.Count);
            // Do we get the most recent message first?
            Assert.Equal("Sorry, something came up.", messages[0].Body);
            // Only contains messages between the provided recipient and sender.
            Assert.Empty(messages.FindAll(m => m.Sender == "spike_bulldog" || m.Recipient == "spike_bulldog"));
        }

        [Fact]
        public void GetMessagesBetweenPartiesSinceDaysAgo_ReturnsAllMessagesSentInTimePeriod()
        {
            // Test database preloaded with four messages between tom and jerry, along with one message from spike_bulldog to tom.

            var messages = _snowflakeDbService.GetMessagesBetweenPartiesSinceDaysAgo("tom", "jerry", 3);

            // Do we get the correct number of messages back?
            Assert.Equal(1, messages.Count);
            // Do we get the most recent message first?
            Assert.Equal("Sorry, something came up.", messages[0].Body);
            // Only contains messages between the provided recipient and sender.
            Assert.True(messages[0].Sender != "spike_bulldog" || messages[0].Recipient != "spike_bulldog");
        }

        [Fact]
        public void GetMessagesAllSendersSinceDaysAgo_ReturnsAllMessagesSentInTimePeriod()
        {
            // Test database preloaded with four messages between tom and jerry, along with one message from spike_bulldog to tom.

            var messages = _snowflakeDbService.GetMessagesAllSendersSinceDaysAgo(3);

            // Do we get the correct number of messages back?
            Assert.Equal(2, messages.Count);
            // Do we get the most recent message first?
            Assert.Equal("Can you help me with something outside, Tom?", messages[0].Body);
        }

        [Fact]
        public void GetMessagesAllSendersWithLimit_ReturnsLimitedNumberOfMessages()
        {
            // Test database preloaded with four messages between tom and jerry, along with one message from spike_bulldog to tom.

            var messages = _snowflakeDbService.GetMessagesAllSendersWithLimit(3);

            // Do we get the correct number of messages back?
            Assert.Equal(3, messages.Count);
            // Do we get the most recent message first?
            Assert.Equal("Can you help me with something outside, Tom?", messages[0].Body);
        }
    }
}
