using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ElStore.Models;
using ElStore.Data;

namespace ElStore.Controllers;

public class LaptopsController : Controller
{

    //private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public LaptopsController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        //_db = db;
        _webHostEnvironment = webHostEnvironment;
    }



    public IActionResult Index()
    {
        return View();
    }
}