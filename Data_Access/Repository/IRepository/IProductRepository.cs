using Models;

namespace Data_Access.Repository.IRepository;

public interface IProductRepository : IRepository<Product>
{
    void Update(Product obj);
}