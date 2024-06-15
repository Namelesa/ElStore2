using Microsoft.AspNetCore.Mvc;
using ElStore.Data;
using ElStore.Models;
using ElStore.Models.ViewModel;
using ElStore.Utility;

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
                var imgPaths = new List<string?>();
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
                            
                            List<string?> deletedImages = HttpContext.Request.Form["deletedImages"]
                                .Select(di => di != null && di.StartsWith("/") ? di.Substring(1) : di)
                                .ToList();
    
                            List<IFormFile> editImages = HttpContext.Request.Form.Files
                                .Where(file => file.Name.StartsWith("editImages"))
                                .ToList();
                            
                            List<IFormFile> addImages = HttpContext.Request.Form.Files
                                .Where(file => file.Name.StartsWith("addImages"))
                                .ToList();
                            
                            List<string?> index = HttpContext.Request.Form["replaceIndexes"].ToList();
                            var indices = Array.ConvertAll(index.ToArray(), int.Parse);
                            string webRootPath = _webHostEnvironment.WebRootPath;
                            
                            
                            if (!deletedImages.Equals(null))
                            {
                                existingProductImages.Image = existingProductImages.Image
                                    .Where(image => !deletedImages.Any(deletedImage => image != null && image.Equals(deletedImage, StringComparison.OrdinalIgnoreCase)))
                                    .ToList();

                                foreach (var image in deletedImages)
                                {
                                    if (image != null)
                                    {
                                        var fullPath = Path.Combine(webRootPath, image);
                                        if (System.IO.File.Exists(fullPath))
                                        {
                                            System.IO.File.Delete(fullPath);
                                        }
                                    }
                                }
                            }

                            if (!addImages.Equals(null))
                            {
                                foreach (var image in addImages)
                                {
                                    string fileName = Guid.NewGuid().ToString();
                                    string extension = Path.GetExtension(image.FileName);
                                    string filePath = Path.Combine(webRootPath, WC.ImagePath, fileName + extension);

                                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                                    {
                                        image.CopyTo(fileStream);
                                    }

                                    existingProductImages.Image.Add(Path.Combine(WC.ImagePath, fileName + extension));
                                }
                            }

                            if (!editImages.Equals(null))
                            {
                                var imgPaths = new List<string?>();
                                
                                foreach (var image in editImages)
                                {
                                    string fileName = Guid.NewGuid().ToString();
                                    string extension = Path.GetExtension(image.FileName);
                                    string filePath = Path.Combine(webRootPath, WC.ImagePath, fileName + extension);

                                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                                    {
                                        image.CopyTo(fileStream);
                                    }
                                    imgPaths.Add(Path.Combine(WC.ImagePath, fileName + extension));
                                }
                                for (int i = 0; i < indices.Length; i++)
                                {
                                    int l = indices[i];
            
                                    if (l >= 0 && l < existingProductImages.Image.Count)
                                    {
                                        existingProductImages.Image[l] = imgPaths[i];
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            
                            if (phone.Video != null && phone.Video.Contains("https"))
                            {
                                string[] parts = phone.Video.Split('=');
                                string videoId = parts[^1];
                                existingProductImages.Video = videoId;
                            }
                            
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
        
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null &&
            HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
        {
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
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
        
        foreach (var item in shoppingCartList)
        {
            if (item.ProductId == id)
            {
                detailsVm.ExistInCard = true;
            }
        }
        
        return View(detailsVm);
    }
    
    //Details - post
    [HttpPost, ActionName("Details")]
    public IActionResult DetailsPost(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null &&
            HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
        {
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        }
        shoppingCartList.Add(new ShoppingCart{ProductId = id});
        HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
        return RedirectToAction(nameof(Index));
    }
    
    //Remove from Cart
    public IActionResult RemoveFromCart(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null &&
            HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
        {
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        }

        var itemToRemove = shoppingCartList.SingleOrDefault(u => u.ProductId == id);

        if (itemToRemove != null)
        {
            shoppingCartList.Remove(itemToRemove);
        }
        
        HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
        return RedirectToAction(nameof(Index));
    }
    
    //Delete
    public IActionResult Delete(int? id)
    {
        string webRootPath = _webHostEnvironment.WebRootPath;
        var product = _db.Product.Find(id);
        if (product != null)
        {
            var descriptions = _db.DescriptionPC.FirstOrDefault(u => u.Id == product.DescriptionPCId);
            var imageDb = _db.Images.Find(product.ImageId);
        
            if (descriptions != null)
            {
                _db.DescriptionPC.Remove(descriptions);
            }
            
            var imagesForProduct = _db.Images.Where(u => u.Id == product.ImageId).ToList();
            
            var imagePaths = imagesForProduct.SelectMany(u => u.Image).ToList();
        
            foreach (var imagePath in imagePaths)
            {
                if (imagePath != null)
                {
                    var fullPath = Path.Combine(webRootPath, imagePath);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
            }

            if (imageDb != null) _db.Images.Remove(imageDb);
            _db.Product.Remove(product);
            _db.SaveChanges();
        }
        return RedirectToAction(nameof(Index));
    }
}