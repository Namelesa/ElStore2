using System.ComponentModel.DataAnnotations;

namespace ElStore.Models.ViewModel;

public class ProductUserVM
{
    public ProductUserVM()
    {
        ProductList = new List<ProductVM>();
    }
    
    public AllUser User { get; set; }
    public IList<ProductVM> ProductList { get; set; }
    public double Total { get; set; }
    
    [Required] public string City { get; set; }
    [Required] public string NumberOffice { get; set; }
}