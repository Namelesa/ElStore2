using Microsoft.AspNetCore.Mvc;
using ElStore.Data;
using ElStore.Models;
using ElStore.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace ElStore.Controllers;

public class PhonesController : Controller
{

    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public PhonesController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }
    
    public IActionResult Index()
    {
        IQueryable<ProductVM> productVmQuery = from product in _db.Product
            join category in _db.Category on product.CategoryId equals category.Id
            where category.Id == 1
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
        
        var video = _db.Images.Where(u => u.Id == id).Select(i => i.Video).FirstOrDefault();
        var images = _db.Images.Where(i => i.Id == id).Select(i => i.Image).ToList();
        Product? product = _db.Product.Find(id);
        DetailsVM phone = new DetailsVM
        {
            Product = product,
            DescriptionPc = _db.DescriptionPC.FirstOrDefault(d => d != null && d.Id == id),
            HearphoneDescriptions = null,
            Video = video,
            Image = images
        };
        
        if (id == null)
        {
            return View(phone);
        }
        else
        {
            phone.Product = _db.Product.Find(id);
            if (phone.Product == null)
            {
                return NotFound();
            }
            return View(phone);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(DetailsVM phone, List<IFormFile> imageFiles, IFormFile videoFile, int? id)
    {
        var video = _db.Images.Where(u => u.Id == id).Select(i => i.Video).FirstOrDefault();
        var images = _db.Images.Where(i => i.Id == id).Select(i => i.Image).ToList();
        Product? product = _db.Product.Find(id);
        DetailsVM phones = new DetailsVM
        {
            Product = product,
            DescriptionPc = _db.DescriptionPC.FirstOrDefault(d => d != null && d.Id == id),
            HearphoneDescriptions = null,
            Video = video,
            Image = images
        };
        
        if (!ModelState.IsValid)
        {
            if (phone.Product != null && phone.Product.Id == 0)
            {
                //create a product
                Console.WriteLine("Create");
                var existingCategory = _db.Category.Find(phone.Product.CategoryId);
                if (existingCategory != null)
                {
                    _db.Product.Add(phone.Product);
                    _db.SaveChanges();
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                //update
                Console.WriteLine("Update");
                if (phones.Product != null && phones.DescriptionPc != null)
                {
                    phones.Product.Brand = phone.Product.Brand;
                    phones.Product.Model = phone.Product.Model;
                    phones.Product.ShortDescription = phone.Product.ShortDescription;
                    phones.Product.Battery = phone.Product.Battery;
                    phones.Product.Price = phone.Product.Price;
                    phones.DescriptionPc.RAM = phone.Product.DescriptionPC.RAM;
                    phones.DescriptionPc.ROM = phone.Product.DescriptionPC.ROM;
                    phones.DescriptionPc.Display = phone.Product.DescriptionPC.Display;
                    phones.DescriptionPc.FrontCamera = phone.Product.DescriptionPC.FrontCamera;
                    phones.DescriptionPc.BackCamera = phone.Product.DescriptionPC.BackCamera;
                    phones.DescriptionPc.Text = phone.Product.DescriptionPC.Text;
                    phones.Video = "n0b4y9snzCQ";
                    _db.SaveChanges();
                }
                

                _db.SaveChanges();
            }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        else
        {
            return NotFound();
        }
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
            .Where(i => i.Id == id)
            .Select(i => i.Image)
            .ToList();

        var video = _db.Images.Where(u=> u.Id == id).Select(i => i.Video).FirstOrDefault();
        
        DetailsVM detailsVm = new DetailsVM()
        {
            Product = product,
            Image = images,
            Video = video,
            DescriptionPc = _db.DescriptionPC.FirstOrDefault(d => d != null && d.Id == id),
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