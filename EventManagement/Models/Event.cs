using System;
using System.Collections.Generic;

namespace EventManagement.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime EventDateTime { get; set; }
        public int MaxParticipants { get; set; }
        public string Status { get; set; }

        public int CreatedBy { get; set; }
        public User Creator { get; set; }

        public ICollection<Registration> Registrations { get; set; }
    }
}
