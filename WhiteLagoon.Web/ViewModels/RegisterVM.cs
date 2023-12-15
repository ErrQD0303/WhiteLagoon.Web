using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WhiteLagoon.Web.ViewModels
{
    public class RegisterVM
    {
        [Required]
        public string Email { get; set; }
        
        [Required]
        [DataType(DataType.Password)] //This will hide the password when typing
        public string Password { get; set; }
        
        [Required]
        [DataType(DataType.Password)] //This will hide the password when typing
        [Compare(nameof(Password))] // Compare the password with the ConfirmPassword
        public string ConfirmPassword { get; set; }
        
        [Required]
        public string Name { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        public string? RedirectURL { get; set; }
        public string? Role { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> RoleList { get; set; }
    }
}
