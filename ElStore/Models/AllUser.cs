using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ElStore.Models;

public class AllUser : IdentityUser
{
    [Key]
    public Guid Id { get; set; }

    public string JWT { get; set; }
    public string Login { get; set; }
}