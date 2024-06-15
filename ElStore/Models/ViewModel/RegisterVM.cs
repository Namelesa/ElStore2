using System.ComponentModel.DataAnnotations;

namespace ElStore.Models.ViewModel;

public class RegisterVM
{
    [Microsoft.Build.Framework.Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Microsoft.Build.Framework.Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    [Microsoft.Build.Framework.Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }
}
