using Data_Access.Data;
using Data_Access.Repository.IRepository;
using Models;

namespace Data_Access.Repository;

public class DescriptionPcRepository : Repository<DescriptionPC>, IDescriptionPcRepository
{
    private readonly ApplicationDbContext _db;

    public DescriptionPcRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }

    public void Update(DescriptionPC obj)
    {
        var objFromDb = base.FirstOrDefault(u=> u.Id == obj.Id);

        if (objFromDb != null)
        {
            objFromDb.RAM = obj.RAM;
            objFromDb.ROM = obj.ROM;
            objFromDb.Display = obj.Display;
            objFromDb.FrontCamera = obj.FrontCamera;
            objFromDb.BackCamera = obj.BackCamera;
            objFromDb.Processor = obj.Processor;
            objFromDb.Text = obj.Text;
            
        }
    }
}