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

        [Required(ErrorMessage = "Please select a payment method.")]
        public string PaymentMethod { get; set; }
    }
}
