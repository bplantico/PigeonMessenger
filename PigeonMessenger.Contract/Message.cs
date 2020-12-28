using System;

namespace PigeonMessenger.Contract
{
    public class Message
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Body { get; set; }
        public bool IsPublic { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
