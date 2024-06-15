using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace ElStore.Models.ViewModel;

public class ProductVM
{
    public ProductVM()
    {
        ExistInCard = false;
        Count = 1;
    }
    public Product Product { get; set; }
    [ValidateNever]
    public List<string?> Image { get; set; }
    
    public bool ExistInCard { get; set; }

    [NotMapped]
    [Range(1, 10000, ErrorMessage = "Count must be > than 0")]
    public int Count { get; set; }
}