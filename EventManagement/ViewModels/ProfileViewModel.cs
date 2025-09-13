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
    }
}