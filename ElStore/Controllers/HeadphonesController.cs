using Microsoft.AspNetCore.Mvc;

using ElStore.Data;
using ElStore.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace ElStore.Controllers;

public class HeadphonesController : Controller
{

    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public HeadphonesController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }



    public IActionResult Index()
    {
        IQueryable<ProductVM> productVmQuery = from product in _db.Product
                                               join category in _db.Category on product.CategoryId equals category.Id
                                               where category.Id == 2
                                               select new ProductVM
                                               {
                                                   Product = product,
                                                   Image = product.Images.Image,
                                               };
        
        var productVm = productVmQuery.ToList();

        return View(productVm);
    }
    
    public IActionResult Upsert()
    {
        return View();
    }
    
    public IActionResult Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        
        var product = _db.Product
            .Include(u => u.Category)
            .FirstOrDefault(u => u.Id == id);
        
        var images = _db.Images
            .Where(i => i.Id == id)
            .Select(i => i.Image)
            .ToList();

        DetailsVM detailsVm = new DetailsVM()
        {
            Product = product,
            Image = images,
            DescriptionPc = null,
            HearphoneDescriptions = _db.HearphoneDescriptions.FirstOrDefault(h => h != null && h.Id == id)
        };

        return View(detailsVm);
    }

    [HttpPost, ActionName("Details")]
    public IActionResult DetailsPost(int id, DetailsVM detailsVm)
    {
        return View(detailsVm);
    }
}