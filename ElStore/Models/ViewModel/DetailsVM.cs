using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ElStore.Models.ViewModel;

public class DetailsVM
{
    public Product? Product { get; set; }
    [ValidateNever]
    public IEnumerable<IEnumerable<string>> Image { get; set; }
    [ValidateNever]
    public string Video { get; set; }
    [ValidateNever]
    public DescriptionPC? DescriptionPc { get; set; }
    [ValidateNever]
    public HearphoneDescriptions? HearphoneDescriptions { get; set; }
}