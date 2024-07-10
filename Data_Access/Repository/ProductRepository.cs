using Data_Access.Data;
using Data_Access.Repository.IRepository;
using Models;

namespace Data_Access.Repository;

public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly ApplicationDbContext _db;
    public ProductRepository(ApplicationDbContext db): base(db)
    {
        _db = db;
    }
    
    public void Update(Product obj)
    {
        var objFromDb = _db.Product.FirstOrDefault(p => p.Id == obj.Id);
        if (objFromDb != null)
        {
            objFromDb.Brand = obj.Brand;
            objFromDb.Model = obj.Model;
            objFromDb.Battery = obj.Battery;
            objFromDb.Price = obj.Price;
            objFromDb.ShortDescription = obj.ShortDescription;
            objFromDb.Guarantee = obj.Guarantee;
        }
    }
}