using PigeonMessenger.Contract;
using System.Collections.Generic;

namespace PigeonMessenger
{
    public interface IDbService
    {
        public string CreateMessage(Message message);

        public IEnumerable<Message> GetMessagesBetweenPartiesSinceDaysAgo(string recipient, string sender, int sinceDaysAgo);

        public IEnumerable<Message> GetMessagesAllSendersWithLimit(int limit);
        
        public IEnumerable<Message> GetMessagesAllSendersSinceDaysAgo(int sinceDaysAgo);

        public IEnumerable<Message> GetMessagesBetweenPartiesWithLimit(string recipient, string sender, int limit);
    }
}