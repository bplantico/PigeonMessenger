using System;

namespace PigeonMessenger.Contract
{
    public class Message
    {
        public string Id { get; set; }
        public string Sender { get; set; } // Really this could be an id as an int, long, guid, etc and be named SenderId. While seeing this in action, I wanted to see the names of the sender and receiver.
        public string Recipient { get; set; }
        public string Body { get; set; }
        public bool IsPublic { get; set; } = true; // Perhaps later there's a feature (premium/tiered) that users can send private messages that can't be retrieved by the rest of the world?
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
