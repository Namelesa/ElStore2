using Data_Access.Data;
using Data_Access.Repository.IRepository;
using Models;

namespace Data_Access.Repository;

public class OrderDetailsRepository :  Repository<OrderDetails>, IOrderDetailsRepository
{
    private readonly ApplicationDbContext _db;
    public OrderDetailsRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(OrderDetails obj)
    {
        _db.OrderDetails.Update(obj);
    }
}