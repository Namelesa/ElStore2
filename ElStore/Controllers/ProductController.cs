using Microsoft.AspNetCore.Mvc;
using Data_Access.Data;
using Data_Access.Repository.IRepository;
using Models;
using Models.ViewModel;
using Utility;
using Microsoft.AspNetCore.Authorization;

namespace ElStore.Controllers;

public class ProductController : Controller
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IDescriptionPcRepository _descriptionPc;
    private readonly IHeadphoneDescriptionRepository _headphoneDescription;
    private readonly IProductRepository _prodRepo;
    private readonly ICategoryRepository _category;
    private readonly IImageRepository _imageRepository;

    public ProductController(IWebHostEnvironment webHostEnvironment, IDescriptionPcRepository descriptionPc, IHeadphoneDescriptionRepository headphoneDescription, IProductRepository prodRepo, ICategoryRepository category, IImageRepository imageRepository)
    {
        _webHostEnvironment = webHostEnvironment;
        _descriptionPc = descriptionPc;
        _headphoneDescription = headphoneDescription;
        _prodRepo = prodRepo;
        _category = category;
        _imageRepository = imageRepository;
    }
    
    //Index - get
    public IActionResult Index(string category)
    {
        var categoryName = _category.FirstOrDefault(c => c.Name == category);
        int categoryId = categoryName.Id;

        var productVmQuery = _prodRepo.GetAll(
            filter: p => p.CategoryId == categoryId,
            includeProperties: "Images"
        ).Select(product => new ProductVM
        {
            Product = product,
            Image = product.Images.Image
        }).ToList();

        return View(productVmQuery);
    }
    
    //Upsert - get
    [Authorize(Roles = "Admin")]
    public IActionResult Upsert(int id)
    {
        DetailsVM item = new DetailsVM();
        
        Product? product = _prodRepo.Find(id);
        if (product != null)
        {
            var images = _imageRepository.GetByProductId(product.ImageId);
            var video = _imageRepository.GetVideoByProductId(product.ImageId);
            
            item.Product = product;
            item.DescriptionPc = _descriptionPc.FirstOrDefault(d => d.Id == product.DescriptionPCId);
            item.HearphoneDescriptions = _headphoneDescription.FirstOrDefault(d => d.Id == product.DescriptionHId);
            item.Video = video;
            item.Image = images;
        }
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
                item.Product.CategoryId = categoryId;
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
                
                _imageRepository.Add(image);
                _imageRepository.Save();

                item.Product.ImageId = image.Id;
                _prodRepo.Add(item.Product);
                _prodRepo.Save();
            }
            
            else
            {
                //update
                var existingProduct = _prodRepo.Find(item.Product.Id);
                var existingProductDescription = _descriptionPc.Find(existingProduct.DescriptionPCId);
                var existingProductHeadDescription= _headphoneDescription.Find(existingProduct.DescriptionHId);
                var existingProductImages = _imageRepository.FindImagesById(existingProduct.ImageId);
                
                item.Product.DescriptionPC.Id = existingProductDescription.Id;
                item.Product.HearphoneDescriptions.Id = existingProductHeadDescription.Id;
                
                _prodRepo.Update(item.Product);            
                _descriptionPc.Update(item.Product.DescriptionPC);
                _headphoneDescription.Update(item.Product.HearphoneDescriptions);
                            
                            
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
            _imageRepository.Save();
            _headphoneDescription.Save();
            _descriptionPc.Save();
            _prodRepo.Save();
            
            var categoryName = _category.FirstOrDefault(c => c.Id == categoryId);
            var name = categoryName.Name;
            return RedirectToAction("Index", new { category = name });
        }
        return View(item);
    }

    //Details - get
    public IActionResult Details(int id)
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null &&
            HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
        {
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        } 
        
        var product = _prodRepo.Find(id);
        var categoryName = _category.FirstOrDefault(c => c.Id == product.CategoryId);
        
        product.Category.Name = categoryName.Name;

        DetailsVM detailsVm = new DetailsVM()
        {
            Product = product,
            Image = _imageRepository.GetByProductId(product.ImageId),
            Video = _imageRepository.GetVideoByProductId(product.ImageId),
            DescriptionPc = _descriptionPc.FirstOrDefault(d => d.Id == product.DescriptionPCId),
            HearphoneDescriptions = _headphoneDescription.FirstOrDefault(d => d.Id == product.DescriptionHId)
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

        var product = _prodRepo.Find(id);
        var categoryName = _category.FirstOrDefault(c => c.Id == product.CategoryId);
        var name = categoryName.Name;
        return RedirectToAction("Index", new { category = name });
        
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

        var product = _prodRepo.Find(id);
        var categoryName = _category.FirstOrDefault(c => c.Id == product.CategoryId);
        var name = categoryName.Name;
        return RedirectToAction("Index", new { category = name });
    }
    
    //Delete
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        string webRootPath = _webHostEnvironment.WebRootPath;
        var product = _prodRepo.Find(id);
    
        var descriptions = _descriptionPc.FirstOrDefault(u => u.Id == product.DescriptionPCId);
        var descriptionsH = _headphoneDescription.FirstOrDefault(u => u.Id == product.DescriptionHId);
        var imagesForProduct = _imageRepository.FindImagesById(product.ImageId);
    
        _descriptionPc.Remove(descriptions);
        _headphoneDescription.Remove(descriptionsH);
    
        var imagePaths = imagesForProduct.Image.ToList();
    
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

        _imageRepository.Remove(imagesForProduct);
    
        var categoryName = _category.FirstOrDefault(c => c.Id == product.CategoryId);
        var name = categoryName.Name;
    
        _prodRepo.Remove(product);
        _prodRepo.Save();
    
        return RedirectToAction("Index", new { category = name });
    }
}