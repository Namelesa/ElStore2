using Models;

namespace Data_Access.Repository.IRepository;

public interface IHeadphoneDescriptionRepository : IRepository<HearphoneDescriptions> 
{
    void Update(HearphoneDescriptions obj);
}