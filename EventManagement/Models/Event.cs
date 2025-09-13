using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventManagement.Models
{
    public class Event
    {
        public int EventId { get; set; }
        [Required(ErrorMessage = "Event title is required")]
        public string Title { get; set; }


        [Required(ErrorMessage = "Event description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Event date and time is required")]
        public DateTime EventDateTime { get; set; }

        [Required(ErrorMessage = "Maximum participants is required")]
        public int MaxParticipants { get; set; }
        [Required(ErrorMessage = "Venue is required")]
        [StringLength(100, ErrorMessage = "Venue cannot exceed 100 characters")]
        public string Venue { get; set; }  // <-- Added Venue Field


        public string Status { get; set; }

        [Column(TypeName = "decimal(18, 2)")] // For storing currency
        public decimal Fees { get; set; }
       
        public int CreatedBy { get; set; }
        public User Creator { get; set; }

        public ICollection<Registration> Registrations { get; set; }
    }
}
