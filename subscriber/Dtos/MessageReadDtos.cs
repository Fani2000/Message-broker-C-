using System;

namespace Subscriber.Dtos
{
    public class MessageReadDtos
    {
        public int Id { get; set; }
        public string? TopicMessage { get; set; }
        public DateTime ExpiresAfter { get; set; }
        public string? MessageStatus { get; set; }
    }
}