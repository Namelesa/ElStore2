using System.ComponentModel.DataAnnotations;

namespace ElStore.Models.ViewModel;

public class LoginVM
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required] public string UserName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}