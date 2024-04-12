using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElStore.Models.ViewModel;

public class ProductVM
{
    public Product Product { get; set; }
    [ValidateNever]
    public IEnumerable<string> Image { get; set; }
}