using System.ComponentModel.DataAnnotations;

namespace ElStore.Models.ViewModel
{
    public class LoginVM
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}