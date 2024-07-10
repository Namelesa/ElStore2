using Models;

namespace Data_Access.Repository.IRepository
{
    public interface IImageRepository : IRepository<Images>
    {
        List<List<string?>> GetByProductId(int imageId);
        string? GetVideoByProductId(int imageId);
        
        Images FindImagesById(int id);
        void Update(Images image);
    }
}