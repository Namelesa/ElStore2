using Models;

namespace Data_Access.Repository.IRepository;

public interface IDescriptionPcRepository : IRepository<DescriptionPC> 
{
    void Update(DescriptionPC obj);
}