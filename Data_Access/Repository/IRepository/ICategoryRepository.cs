using Models;

namespace Data_Access.Repository.IRepository;

public interface ICategoryRepository : IRepository<Category> 
{ 
    void Update(Category obj);
    
}