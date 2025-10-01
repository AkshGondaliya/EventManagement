using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace EventManagement.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(30, ErrorMessage = "Full Name cannot exceed 30 characters")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Full Name is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        public string PasswordHash { get; set; } // for store hashed password

        // This is used only during registration and validation
        [NotMapped] // <-- Do NOT save to database
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }
        //[Required(ErrorMessage = "Role is required")]
        //[RegularExpression("^(Admin|EventCoordinator|Student)$", ErrorMessage = "Role must be Admin, EventCoordinator, or Student")]
        public string Role { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string CollegeName { get; set; }
        public ICollection<Registration> Registrations { get; set; }
    }
}
//test@.com not allowed    user_123@college.edu allowed