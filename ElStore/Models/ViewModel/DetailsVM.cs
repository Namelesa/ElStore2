using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ElStore.Models.ViewModel;

public class DetailsVM
{
    public DetailsVM()
    {
        ExistInCard = false;
    }
    public Product? Product { get; set; }
    [ValidateNever]
    public List<List<string?>> Image { get; set; }
    [ValidateNever]
    public string? Video { get; set; }
    [ValidateNever]
    public DescriptionPC? DescriptionPc { get; set; }
    [ValidateNever]
    public HearphoneDescriptions? HearphoneDescriptions { get; set; }

    public bool ExistInCard { get; set; }
}