using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElStore.Models;

public class Product
{
    public Product()
    {
        Count = 1;
    }
    
    [Key] public int Id { get; set; }
    
    [Required]
    public string Brand { get; set; }
    [Required]
    public string Model { get; set; }
    [Range(1, int.MaxValue)]
    public double Price { get; set; }
    
    [Required]
    public string ShortDesc { get; set; }
    
    [Display(Name="Description")]
    public int DescriptionId { get; set; }
    [ForeignKey("DescriptionId")]
    public virtual Description Description { get; set; }
    
    [Display(Name="Category Type")]
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public virtual Category Category{ get; set; }

    [Display(Name="Images")]
    public int ImagesId { get; set; }
    [ForeignKey("ImagesId")]
    public virtual Images Images { get; set; }
    
    [NotMapped]
    [Range(1, 10000, ErrorMessage ="Count must be > than 0")]
    public int Count { get; set; }
    
}