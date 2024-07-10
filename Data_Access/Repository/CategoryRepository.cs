using Data_Access.Data;
using Data_Access.Repository.IRepository;
using Models;

namespace Data_Access.Repository;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    private readonly ApplicationDbContext _db;

    public CategoryRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }
    
    public void Update(Category obj)
    {
        var objFromDb = _db.Category.FirstOrDefault(c => c.Id == obj.Id);
        if (objFromDb != null)
        {
            objFromDb.Name = obj.Name;
        }
    }
}