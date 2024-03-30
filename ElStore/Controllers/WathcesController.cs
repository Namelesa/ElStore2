using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ElStore.Models;
using ElStore.Data;

namespace ElStore.Controllers;

public class WatchesController : Controller
{

    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public WatchesController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }



    public IActionResult Index()
    {
        IEnumerable<Product> watch = _db.Products.Where(u => u.CategoryId == 3);
        return View(watch);
    }
}