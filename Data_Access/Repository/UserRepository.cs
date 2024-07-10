using Data_Access.Data;
using Data_Access.Repository.IRepository;
using Models;

namespace Data_Access.Repository;

public class UserRepository : Repository<AllUser>, IUserRepository
{
    private readonly ApplicationDbContext _db;
    public UserRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }
}