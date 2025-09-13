using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.ViewModels
{
    public class ProfileViewModel
    {
        [Required]
        [StringLength(50)]
        public string FullName { get; set; }

        [Url]
        [Display(Name = "Profile Picture URL")]
        public string ProfilePictureUrl { get; set; }

        // ADD THIS PROPERTY to accept the uploaded file
        [Display(Name = "Upload New Profile Image")]
        public IFormFile ProfileImage { get; set; }

        [StringLength(100)]
        [Display(Name = "College Name")]
        public string CollegeName { get; set; } 
    }
}