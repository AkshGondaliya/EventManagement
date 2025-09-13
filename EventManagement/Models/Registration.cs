using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Models
{
    public class Registration
    {
        public int RegistrationId { get; set; }

        public int UserId { get; set; }
        public int EventId { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Status { get; set; }

        [StringLength(50)]
        public string Semester { get; set; } // Added Semester

        [StringLength(50)]
        public string Branch { get; set; } // Added Branch

        public User User { get; set; }
        public Event Event { get; set; }
    }
}
