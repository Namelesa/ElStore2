using Microsoft.AspNetCore.Mvc;
using ElStore.Data;
using ElStore.Models;
using NuGet.Common;

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
        IEnumerable<Product> phone = _db.Products.Where(u=>u.CategoryId == 1);
        return View(phone);
    }
}