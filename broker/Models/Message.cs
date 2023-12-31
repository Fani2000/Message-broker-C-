using System;
using System.ComponentModel.DataAnnotations;

namespace MessageBroker.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? TopicMessage { get; set; }

        [Required]
        public int Subscriptionid { get; set; }

        [Required]
        public DateTime ExpiresAfter { get; set; } = DateTime.Now.AddDays(1);

        [Required]
        public string? MessgageStatus { get; set; } = "NEW";
    }
}