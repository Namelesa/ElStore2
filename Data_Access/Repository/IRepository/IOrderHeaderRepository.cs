using Models;

namespace Data_Access.Repository.IRepository;

public interface IOrderHeaderRepository : IRepository<OrderHeader> 
{
    void Update(OrderHeader obj);
}