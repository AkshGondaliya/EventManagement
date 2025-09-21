using System;

namespace EventManagement.ViewModels
{
    public class ViewTicketViewModel
    {
        public int RegistrationId { get; set; }
        public string EventTitle { get; set; }
        public string AttendeeName { get; set; }
        public DateTime EventDate { get; set; }
        public string Venue { get; set; }
        public string QrCodeImageUrl { get; set; }
    }
}