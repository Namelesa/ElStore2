using Data_Access.Data;
using Data_Access.Repository.IRepository;
using Models;

namespace Data_Access.Repository;

public class HeadphoneDescriptionRepository : Repository<HearphoneDescriptions>, IHeadphoneDescriptionRepository
{
    private readonly ApplicationDbContext _db;

    public HeadphoneDescriptionRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }
    
    public void Update(HearphoneDescriptions obj)
    {
        var objFromDb = base.FirstOrDefault(u=> u.Id == obj.Id);

        if (objFromDb != null)
        {
            objFromDb.SpeakerSize = obj.SpeakerSize;
            objFromDb.Design = obj.Design;
            objFromDb.TypeConnections = obj.TypeConnections;
            objFromDb.Text = obj.Text;
        }
    }
}