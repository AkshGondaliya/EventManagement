using System;

namespace EventManagement.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; } // Who will receive the notification
        public string Message { get; set; }
        public bool IsRead { get; set; } = false; // Track if the user has read the notification
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
