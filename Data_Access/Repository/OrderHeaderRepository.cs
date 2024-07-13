using Data_Access.Data;
using Data_Access.Repository.IRepository;
using Models;

namespace Data_Access.Repository;

public class OrderHeaderRepository :  Repository<OrderHeader>, IOrderHeaderRepository
{
    private readonly ApplicationDbContext _db;
    
    public OrderHeaderRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(OrderHeader obj)
    {
        _db.OrderHeaders.Update(obj);
    }
}