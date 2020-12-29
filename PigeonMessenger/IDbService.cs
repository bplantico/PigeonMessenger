// Disable all XML Comment warnings in this file //
#pragma warning disable 1591

using PigeonMessenger.Contract;
using System.Collections.Generic;

namespace PigeonMessenger
{
    public interface IDbService
    {
        public string CreateMessage(Message message);

        public List<Message> GetMessagesBetweenPartiesSinceDaysAgo(string recipient, string sender, int sinceDaysAgo);

        public List<Message> GetMessagesAllSendersWithLimit(int limit);
        
        public List<Message> GetMessagesAllSendersSinceDaysAgo(int sinceDaysAgo);

        public List<Message> GetMessagesBetweenPartiesWithLimit(string recipient, string sender, int limit);

        public void DeleteMessage(string id);
    }
}