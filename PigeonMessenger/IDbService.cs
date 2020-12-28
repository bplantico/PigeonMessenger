using PigeonMessenger.Contract;
using System.Collections.Generic;

namespace PigeonMessenger
{
    public interface IDbService
    {
        internal string CreateMessage(Message message);

        internal IEnumerable<Message> GetMessagesBetweenPartiesSinceDaysAgo(string recipient, string sender, int sinceDaysAgo);

        internal IEnumerable<Message> GetMessagesAllSendersWithLimit(int limit);
        
        internal IEnumerable<Message> GetMessagesAllSendersSinceDaysAgo(int sinceDaysAgo);

        internal IEnumerable<Message> GetMessagesBetweenPartiesWithLimit(string recipient, string sender, int limit);
    }
}