using Microsoft.AspNetCore.Mvc;
using ElStore.Data;
using ElStore.Models;
using ElStore.Models.ViewModel;

namespace ElStore.Controllers;

public class LaptopsController : Controller
{

    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public LaptopsController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }
    
    //Index - get
    public IActionResult Index()
    {
        IQueryable<ProductVM> productVmQuery = from product in _db.Product
            join category in _db.Category on product.CategoryId equals category.Id
            where category.Id == 4
            select new ProductVM
            {
                Product = product,
                Image = product.Images.Image
            };
        
        List<ProductVM> productVm = productVmQuery.ToList();

        return View(productVm);
    }
    
    //Upsert - get
    public IActionResult Upsert(int? id)
    {
        DetailsVM phone = new DetailsVM();
        if (id == null)
        {
            return View(phone);
        }

        Product? product = _db.Product.Find(id);
        if (product == null)
        {
            return NotFound();
        }

        var video = _db.Images.Where(u => u.Id == product.ImageId).Select(i => i.Video).FirstOrDefault();
        var images = _db.Images.Where(i => i.Id == product.ImageId).Select(i => i.Image).ToList();

        phone.Product = product;
        phone.DescriptionPc = _db.DescriptionPC.FirstOrDefault(d => d.Id == product.DescriptionPCId);
        phone.HearphoneDescriptions = null;
        phone.Video = video;
        phone.Image = images;

        return View(phone);
    }

    //Upsert - post
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(DetailsVM phone)
    {
    
        if (phone.Product != null)
        {
            if (phone.Product.Id == 0)
            {
                //create
                phone.Product.CategoryId = 4;
                phone.Product.DescriptionHId = 1;
                var files = HttpContext.Request.Form.Files.Where(file => file.Name.StartsWith("imageFiles"));
                string webRootPath = _webHostEnvironment.WebRootPath;
                var imgPaths = new List<string>();
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        string fileName = Guid.NewGuid().ToString();
                        string extension = Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(webRootPath, WC.ImagePath, fileName + extension);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        imgPaths.Add(Path.Combine(WC.ImagePath, fileName + extension));
                    }
                }

                Images image = new Images { Image = imgPaths };

                if (phone.Video != null)
                {
                    if (phone.Video.Contains(".mp4"))
                    {
                        var video = HttpContext.Request.Form.Files.FirstOrDefault(file => file.Name == "videoFile");
                        string fileName = Guid.NewGuid().ToString();
                        if (video != null)
                        {
                            string extension = Path.GetExtension(video.FileName);
                            string uploadVideo = Path.Combine(webRootPath, WC.VideoPath);
                            string videoFilePath = Path.Combine(uploadVideo, fileName + extension);

                            using (var fileStream = new FileStream(videoFilePath, FileMode.Create))
                            {
                                video.CopyTo(fileStream);
                            }

                            image.Video = Path.Combine(WC.VideoPath, fileName + extension);
                        }
                    }
                    
                    if (phone.Video.Contains("https"))
                    {
                        string[] parts = phone.Video.Split('=');
                        string videoId = parts[^1];
                        image.Video = videoId;
                    }
                }
                
                _db.Images.Add(image);
                _db.SaveChanges();

                phone.Product.ImageId = image.Id;
                _db.Product.Add(phone.Product);
                _db.SaveChanges();
            }
            
            else
            {
                //update
                var existingProduct = _db.Product.Find(phone.Product.Id);
            
                    if (existingProduct != null)
                    {
                        var existingProductDescription = _db.DescriptionPC.Find(existingProduct.DescriptionPCId);
                        var existingProductImages = _db.Images.Find(existingProduct.ImageId);
                        if (existingProductDescription != null && existingProductImages != null)
                        {
                            existingProduct.Brand = phone.Product.Brand;
                            existingProduct.Model = phone.Product.Model;
                            existingProduct.Battery = phone.Product.Battery;
                            existingProduct.Price = phone.Product.Price;
                            
                            existingProductDescription.RAM = phone.Product.DescriptionPC.RAM;
                            existingProductDescription.ROM = phone.Product.DescriptionPC.ROM;
                            existingProductDescription.Display = phone.Product.DescriptionPC.Display;
                            existingProductDescription.FrontCamera = phone.Product.DescriptionPC.FrontCamera;
                            existingProductDescription.BackCamera = phone.Product.DescriptionPC.BackCamera;
                            existingProductDescription.Text = phone.Product.DescriptionPC.Text;
                                
                            if (phone.Video != null) existingProductImages.Video = phone.Video;
                            
                            
                            
                        }
                    }
                    else
                    {
                        return NotFound();
                    }
            }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(phone);
    }
    
    //Details - get
    public IActionResult Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = _db.Product.Find(id);
        
        var images = _db.Images
            .Where(i => product != null && i.Id == product.ImageId)
            .Select(i => i.Image)
            .ToList();

        var video = _db.Images.Where(u=> product != null && u.Id == product.ImageId).Select(i => i.Video).FirstOrDefault();
        
        DetailsVM detailsVm = new DetailsVM()
        {
            Product = product,
            Image = images,
            Video = video,
            DescriptionPc = _db.DescriptionPC.FirstOrDefault(d => product != null && d.Id == product.DescriptionPCId),
            HearphoneDescriptions = null
        };

        return View(detailsVm);
    }
    
    //Details - post
    [HttpPost, ActionName("Details")]
    public IActionResult DetailsPost(int id, DetailsVM detailsVm)
    {
        return View(detailsVm);
    }
}