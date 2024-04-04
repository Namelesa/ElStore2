using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using ElStore.Data;
using ElStore.Models.ViewModel;

namespace ElStore.Controllers;

public class HearphonesController : Controller
{

    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public HearphonesController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }



    public IActionResult Index()
    {
        IQueryable<ProductVM> productVMQuery = from product in _db.Product
                                               join category in _db.Category on product.CategoryId equals category.Id
                                               where category.Id == 2
                                               select new ProductVM
                                               {
                                                   Product = product,
                                                   Category = category.Name,
                                                   Image = product.Images.Image,
                                                   Video = product.Images.Video,
                                                   DescriptionPc = product.DescriptionPC
                                               };
        
        var productVM = productVMQuery.ToList();

        return View(productVM);
    }
}