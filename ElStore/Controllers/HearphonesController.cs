using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ElStore.Models;
using ElStore.Data;

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
        IEnumerable<Product> hearphone = _db.Products.Where(u => u.CategoryId == 2);
        return View(hearphone);
    }
}