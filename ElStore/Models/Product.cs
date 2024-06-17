using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElStore.Models;

public class Product
{
    public Product()
    {
        Count = 1;
    }
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Brand { get; set; }
    public string Model { get; set; }
    public string ShortDescription { get; set; }
    public int Battery { get; set; }
    [Range(1,int.MaxValue)]
    public double Price { get; set; }
    
    [Display(Name ="Category Type")]
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public virtual Category Category { get; set; }
    
    [Display(Name ="Image Type")]
    public int ImageId { get; set; }
    [ForeignKey("ImageId")]
    public virtual Images Images { get; set; }
    
    [Display(Name ="DescriptionPC Type")]
    public int DescriptionPCId { get; set; }
    [ForeignKey("DescriptionPCId")]
    public virtual DescriptionPC DescriptionPC { get; set; }
    
    [Display(Name ="DescriptionH Type")]
    public int DescriptionHId { get; set; }
    [ForeignKey("DescriptionHId")]
    public virtual HearphoneDescriptions HearphoneDescriptions { get; set; }

    public string Guarantee { get; set; }
    
    [NotMapped]
    [Range(1, 10000, ErrorMessage = "Count must be > than 0")]
    public int Count { get; set; }
}