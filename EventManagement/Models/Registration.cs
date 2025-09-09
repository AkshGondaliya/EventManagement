using System;

namespace EventManagement.Models
{
    public class Registration
    {
        public int RegistrationId { get; set; }

        public int UserId { get; set; }
        public int EventId { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Status { get; set; }

        public User User { get; set; }
        public Event Event { get; set; }
    }
}
