using Microsoft.AspNetCore.Mvc;
using ElStore.Data;
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




    [HttpPost, ActionName("Details")]
    public IActionResult DetailsPost(int id, DetailsVM detailsVm)
    {
        return View(detailsVm);
    }
    
    

}