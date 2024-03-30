using System.ComponentModel.DataAnnotations;

namespace ElStore.Models;

public class Images
{
    [Key]
    public int Id { get; set; }
    
    public IEnumerable<string> Image { get; set; }
    public string Video { get; set; }
}