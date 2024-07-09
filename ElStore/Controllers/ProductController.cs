using Microsoft.AspNetCore.Mvc;
using Data_Access.Data;
using Models;
using Models.ViewModel;
using Utility;
using Microsoft.AspNetCore.Authorization;

namespace ElStore.Controllers;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }
    
    //Index - get
    public IActionResult Index(string category)
    {
        var categoryName = _db.Category.FirstOrDefault(c => c.Name == category);
        if (categoryName == null)
        {
            return NotFound();
        }

        int categoryId = categoryName.Id;

        IQueryable<ProductVM> productVmQuery = from product in _db.Product
            join cat in _db.Category on product.CategoryId equals cat.Id
            where cat.Id == categoryId
            select new ProductVM
            {
                Product = product,
                Image = product.Images.Image
            };

        List<ProductVM> productVm = productVmQuery.ToList();
        return View(productVm);
    }
    
    //Upsert - get
    [Authorize(Roles = "Admin")]
    public IActionResult Upsert(int? id)
    {
        DetailsVM item = new DetailsVM();
        if (id == null)
        {
            return View(item);
        }

        Product? product = _db.Product.Find(id);
        if (product == null)
        {
            return NotFound();
        }

        var video = _db.Images.Where(u => u.Id == product.ImageId).Select(i => i.Video).FirstOrDefault();
        var images = _db.Images.Where(i => i.Id == product.ImageId).Select(i => i.Image).ToList();

        item.Product = product;
        item.DescriptionPc = _db.DescriptionPC.FirstOrDefault(d => d.Id == product.DescriptionPCId);
        item.HearphoneDescriptions = _db.HearphoneDescriptions.FirstOrDefault(d => d.Id == product.DescriptionHId);
        item.Video = video;
        item.Image = images;

        return View(item);
    }

    //Upsert - post
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public IActionResult Upsert(DetailsVM item, int categoryId)
    {
        if (item.Product != null)
        {
            if (item.Product.Id == 0)
            {
                //create
                item.Product.Id = categoryId;
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

                if (item.Video != null)
                {
                    if (item.Video.Contains("https"))
                    {
                        string[] parts = item.Video.Split('=');
                        string videoId = parts[^1];
                        image.Video = videoId;
                    }
                }
                
                _db.Images.Add(image);
                _db.SaveChanges();

                item.Product.ImageId = image.Id;
                _db.Product.Add(item.Product);
                _db.SaveChanges();
            }
            
            else
            {
                //update
                var existingProduct = _db.Product.Find(item.Product.Id);
            
                    if (existingProduct != null)
                    {
                        var existingProductDescription = _db.DescriptionPC.Find(existingProduct.DescriptionPCId);
                        var existingProductHeadDescription= _db.HearphoneDescriptions.Find(existingProduct.DescriptionHId);
                        var existingProductImages = _db.Images.Find(existingProduct.ImageId);
                        if (existingProductDescription != null && existingProductImages != null && existingProductHeadDescription != null)
                        {
                            existingProduct.Brand = item.Product.Brand;
                            existingProduct.Model = item.Product.Model;
                            existingProduct.Battery = item.Product.Battery;
                            existingProduct.Price = item.Product.Price;
                            existingProduct.ShortDescription = item.Product.ShortDescription;
                            existingProduct.Guarantee = item.Product.Guarantee;
        
                            existingProductDescription.RAM = item.Product.DescriptionPC.RAM;
                            existingProductDescription.ROM = item.Product.DescriptionPC.ROM;
                            existingProductDescription.Display = item.Product.DescriptionPC.Display;
                            existingProductDescription.FrontCamera = item.Product.DescriptionPC.FrontCamera;
                            existingProductDescription.BackCamera = item.Product.DescriptionPC.BackCamera;
                            existingProductDescription.Processor = item.Product.DescriptionPC.Processor;
                            existingProductDescription.Text = item.Product.DescriptionPC.Text;
                            
                            existingProductHeadDescription.SpeakerSize = item.Product.HearphoneDescriptions.SpeakerSize;
                            existingProductHeadDescription.Design = item.Product.HearphoneDescriptions.Design;
                            existingProductHeadDescription.TypeConnections = item.Product.HearphoneDescriptions.TypeConnections;
                            existingProductHeadDescription.Text = item.Product.HearphoneDescriptions.Text;
                            
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
                            
                            if (item.Video != null && item.Video.Contains("https"))
                            {
                                string[] parts = item.Video.Split('=');
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
            
            var categoryName = _db.Category.FirstOrDefault(c => c.Id == categoryId);
            if (categoryName != null)
            {
                var name = categoryName.Name;
                return RedirectToAction("Index", new { category = name });
            }
        }
        return View(item);
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
        var categoryName = _db.Category.FirstOrDefault(c => product != null && c.Id == product.CategoryId);
        if (categoryName != null && product!= null)
            product.Category.Name = categoryName.Name;

        DetailsVM detailsVm = new DetailsVM()
        {
            Product = product,
            Image = _db.Images
                .Where(i => product != null && i.Id == product.ImageId)
                .Select(i => i.Image)
                .ToList(),
            Video = _db.Images.Where(u=> product != null && u.Id == product.ImageId).Select(i => i.Video).FirstOrDefault(),
            DescriptionPc = _db.DescriptionPC.FirstOrDefault(d => product != null && d.Id == product.DescriptionPCId),
            HearphoneDescriptions = _db.HearphoneDescriptions.FirstOrDefault(d => product != null && d.Id == product.DescriptionHId)
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
        shoppingCartList.Add(new ShoppingCart { ProductId = id });
        HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

        var product = _db.Product.Find(id);
        if (product != null)
        {
            var categoryName = _db.Category.FirstOrDefault(c => c.Id == product.CategoryId);
            if (categoryName != null)
            {
                var name = categoryName.Name;
                return RedirectToAction("Index", new { category = name });
            }
        }

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

        var product = _db.Product.Find(id);
        if (product != null)
        {
            var categoryName = _db.Category.FirstOrDefault(c => c.Id == product.CategoryId);
            if (categoryName != null)
            {
                var name = categoryName.Name;
                return RedirectToAction("Index", new { category = name });
            }
        }

        return RedirectToAction(nameof(Index));
    }
    
    //Delete
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int? id)
    {
        string webRootPath = _webHostEnvironment.WebRootPath;
        var product = _db.Product.Find(id);
        if (product != null)
        {
            var descriptions = _db.DescriptionPC.FirstOrDefault(u => u.Id == product.DescriptionPCId);
            var descriptionsH = _db.HearphoneDescriptions.FirstOrDefault(u => u.Id == product.DescriptionHId);
            var imageDb = _db.Images.Find(product.ImageId);
        
            if (descriptions != null && descriptionsH != null)
            {
                _db.DescriptionPC.Remove(descriptions);
                _db.HearphoneDescriptions.Remove(descriptionsH);
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
            var categoryName = _db.Category.FirstOrDefault(c => c.Id == product.CategoryId);
            if (categoryName != null)
            {
                var name = categoryName.Name;
                _db.Product.Remove(product);
                _db.SaveChanges();
                return RedirectToAction("Index", new { category = name });
            }
        }
        return RedirectToAction("Index","Home");
    }
}