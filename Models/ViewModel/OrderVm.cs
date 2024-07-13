namespace Models.ViewModel;

public class OrderVm
{
    public IEnumerable<OrderHeader> OrderHeader { get; set; }
    public IEnumerable<OrderDetails> OrderDetails { get; set; }
}