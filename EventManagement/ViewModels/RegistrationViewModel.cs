using System.ComponentModel.DataAnnotations;
using EventManagement.Models;

namespace EventManagement.ViewModels
{
    public class RegistrationViewModel
    {

        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public decimal EventFees { get; set; }

        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        public string UserCollegeName { get; set; }

        [Required(ErrorMessage = "Semester is required.")]
        [StringLength(50)]
        public string Semester { get; set; }

        [Required(ErrorMessage = "Branch is required.")]
        [StringLength(50)]
        public string Branch { get; set; }

        [Required(ErrorMessage = "Please select a payment method.")]
        public string PaymentMethod { get; set; }
    }
}
