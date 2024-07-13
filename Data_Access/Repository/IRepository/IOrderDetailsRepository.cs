using Models;

namespace Data_Access.Repository.IRepository;

public interface IOrderDetailsRepository : IRepository<OrderDetails> 
{
    void Update(OrderDetails obj);
}