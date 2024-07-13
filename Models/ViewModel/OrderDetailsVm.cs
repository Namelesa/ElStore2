namespace Models.ViewModel;

public class OrderDetailsVm
{
    public OrderHeader OrderHeader { get; set; }
    public IEnumerable<OrderDetails> OrderDetailsList { get; set; }
    public IEnumerable<Product> ProductList { get; set; }
    
}