using Data_Access.Data;
using Data_Access.Repository.IRepository;
using Models;

namespace Data_Access.Repository
{
    public class ImageRepository : Repository<Images>, IImageRepository
    {
        private readonly ApplicationDbContext _db;

        public ImageRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public List<List<string?>> GetByProductId(int imageId)
        {
            List<List<string?>> imagesGrouped = _db.Images.Where(i => i.Id == imageId).Select(i => i.Image).ToList();
            return imagesGrouped;
        }
        
        public string? GetVideoByProductId(int imageId)
        {
            return _db.Images.Where(u => u.Id == imageId).Select(i => i.Video).FirstOrDefault();
        }

        public Images FindImagesById(int imageId)
        {
            return _db.Images.FirstOrDefault(u => u.Id == imageId) ?? throw new InvalidOperationException();
        }

        public void Update(Images image)
        {
            var objFromDb = _db.Images.FirstOrDefault(i => i.Id == image.Id);
            if (objFromDb != null)
            {
                objFromDb.Image = image.Image;
                objFromDb.Video = image.Video;
            }
        }
    }
}