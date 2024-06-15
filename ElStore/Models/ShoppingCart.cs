using System.ComponentModel.DataAnnotations;

namespace ElStore.Models;

public class ShoppingCart
{
    public ShoppingCart()
    {
        Count = 1;
    }
    public int ProductId { get; set; }
    
    [Range(1, 10000, ErrorMessage = "Count must be > than 0")]
    public int Count { get; set; }
}