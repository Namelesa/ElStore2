using ElStore.Data;
using ElStore.Models;
using ElStore.Models.ViewModel;
using ElStore.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElStore.Controllers;
public class CartController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CartController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
    {
        _db = db;
        _webHostEnvironment = webHostEnvironment;
    }
    
    //Index get
    public IActionResult Index()
    {
        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
            && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
        {
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        }

        List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();

        var products = _db.Product
            .Where(u => prodInCart.Contains(u.Id)).Include(product => product.Images)
            .AsEnumerable()  // Switch to client evaluation
            .Select(product => new ProductVM
            {
                Product = product,
                Image = product.Images.Image,
                Count = shoppingCartList.FirstOrDefault(x => x.ProductId == product.Id)?.Count ?? 1
            })
            .ToList();

        double totalSum = products.Sum(p => p.Product.Price * p.Count);

        ViewBag.TotalSum = totalSum;

        return View(products);
    }
    
    //Remove from Cart
    public IActionResult Remove(int id)
    {
        List<ShoppingCart?> shoppingCartList = new List<ShoppingCart?>();
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
            && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
        {
            //session exsits
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        }

        shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u != null && u.ProductId == id));
        HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
        return RedirectToAction(nameof(Index));
    }
    public IActionResult ClearCart()
    {
        HttpContext.Session.Set(WC.SessionCart, new List<ShoppingCart>());

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public IActionResult UpdateQuantities(int[] Quantities, int[] ProductIds)
    {
        List<ShoppingCart> shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);

        for (int i = 0; i < ProductIds.Length; i++)
        {
            var item = shoppingCartList.FirstOrDefault(x => x.ProductId == ProductIds[i]);
            if (item != null)
            {
                item.Count = Quantities[i];
            }
        }

        HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

        return RedirectToAction(nameof(Index));
    }
}