using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.ViewModels
{
    public class EventViewModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Event title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Event description is required")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Event date and time is required")]
        [Min24HoursFromNow(ErrorMessage = "Event date and time must be at least 24 hours ahead from now.")]
        public DateTime EventDateTime { get; set; }

        [Required(ErrorMessage = "Maximum participants is required")]
        public int MaxParticipants { get; set; }

        [Required(ErrorMessage = "Venue is required")]
        [StringLength(100)]
        public string Venue { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(50)]
        public string Category { get; set; }

        public decimal Fees { get; set; }
    }

    // Custom validation attribute for 24 hours ahead
    public class Min24HoursFromNowAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime dateTime)
            {
                return dateTime > DateTime.Now.AddHours(24);
            }
            return false;
        }
    }
}