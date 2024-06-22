using System.Security.Claims;
using ElStore.Data;
using ElStore.Models;
using ElStore.Models.ViewModel;
using ElStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElStore.Controllers;
public class CartController : Controller
{
    private readonly ApplicationDbContext _db;
    [BindProperty]
    public ProductUserVM ProductUserVM { get; set; }

    public CartController(ApplicationDbContext db)
    {
        _db = db;
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
            .AsEnumerable() 
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
            var it = _db.Product.FirstOrDefault(u => u.Id == ProductIds[i]);
            if (item != null && it != null)
            {
                item.Count = Quantities[i];
            }
        }

        HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

        return RedirectToAction(nameof(Summary));
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Index")]
    public IActionResult IndexPost()
    {
        return RedirectToAction(nameof(Summary));
    }
    
    [Authorize]
    public IActionResult Summary()
    {
        var claimIdentity = User.Identity as ClaimsIdentity;
        var userLogin = claimIdentity.FindFirst(ClaimTypes.Name);
        var userName = userLogin.Value;

        List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
        if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
            && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
        {
            shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
        }

        List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();

        var products = _db.Product
            .Where(u => prodInCart.Contains(u.Id)).Include(product => product.Images)
            .AsEnumerable()
            .Select(product => 
            {
                var count = shoppingCartList.FirstOrDefault(x => x.ProductId == product.Id)?.Count ?? 1;
                return new ProductVM
                {
                    Product = product,
                    Image = product.Images.Image,
                    Count = count
                };
            })
            .ToList();
        double totalSum = products.Sum(p => p.Product.Price * p.Count);
        ViewBag.TotalSum = totalSum;

        var user = _db.Users.FirstOrDefault(u => u.UserName == userName || u.NormalizedUserName == userName);

        ProductUserVM = new ProductUserVM()
        {
            User = user,
            ProductList = products, 
            Total = totalSum
        };

        return View(ProductUserVM);
    }
}