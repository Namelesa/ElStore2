using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ElStore.Models.ViewModel;

public class ProductVM
{
    public Product Product { get; set; }
    [ValidateNever]
    public string Category { get; set; }
    [ValidateNever]
    public IEnumerable<string> Image { get; set; }
    [ValidateNever]
    public string Video { get; set; }
    [ValidateNever]
    public DescriptionPC DescriptionPc { get; set; }
    [ValidateNever]
    public HearphoneDescriptions HearphoneDescriptions { get; set; }
}