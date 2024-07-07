using System.Security.Claims;
using System.Text;
using Baroque.NovaPoshta.Client.Domain.Address;
using Baroque.NovaPoshta.Client.Services.Address;
using ElStore.Data;
using ElStore.Models;
using ElStore.Models.ViewModel;
using ElStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ElStore.Controllers;
public class CartController(
    ApplicationDbContext db,
    AddressService addressService,
    IWebHostEnvironment webHostEnvironment,
    IEmailSender emailSender)
    : Controller
{
    [BindProperty] private ProductUserVM ProductUserVm { get; set; }

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

        var products = db.Product
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
    public IActionResult UpdateQuantities(int[] quantities, int[] productIds)
    {
        List<ShoppingCart> shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);

        for (int i = 0; i < productIds.Length; i++)
        {
            var item = shoppingCartList.FirstOrDefault(x => x.ProductId == productIds[i]);
            var it = db.Product.FirstOrDefault(u => u.Id == productIds[i]);
            if (item != null && it != null)
            {
                item.Count = quantities[i];
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
        if (User.Identity is ClaimsIdentity claimIdentity)
        {
            var userLogin = claimIdentity.FindFirst(ClaimTypes.Name);
            if (userLogin != null)
            {
                var userName = userLogin.Value;

                List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
                if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                    && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
                {
                    shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
                }

                List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();

                var products = db.Product
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

                var user = db.Users.FirstOrDefault(u => u.UserName == userName || u.NormalizedUserName == userName);

                if (user != null)
                    ProductUserVm = new ProductUserVM()
                    {
                        User = user,
                        ProductList = products,
                        Total = totalSum
                    };
            }
        }

        return View(ProductUserVm);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [ActionName("Summary")]
    public async Task<IActionResult> SummaryPost(ProductUserVM productUserVm)
    {
        var pathToTemplate = Path.Combine(webHostEnvironment.WebRootPath, "templates", "Inquiry.html");
        string htmlBody;
        string subject = "New Inquiry";

        using (StreamReader sr = System.IO.File.OpenText(pathToTemplate))
        {
            htmlBody = await sr.ReadToEndAsync();
        }
        
        string userName = productUserVm.User?.Login ?? "N/A";
        string userEmail = productUserVm.User?.Email ?? "N/A";
        string userPhone = productUserVm.User?.PhoneNumber ?? "N/A";

        StringBuilder productListSb = new StringBuilder();
        double total = 0;

        foreach (var product in productUserVm.ProductList)
        {
            double productSubtotal = product.Product.Price * product.Count;
            total += productSubtotal;

            productListSb.Append($@"
                <div class='product-details'>
                    <span>{product.Product.Brand + " " + product.Product.Model} - Count: {product.Count}</span>
                    <span>Subtotal: ${productSubtotal}</span>
                </div>");
        }

        htmlBody = htmlBody.Replace("{0}", userName)
                        .Replace("{1}", userEmail)
                        .Replace("{2}", userPhone)
                        .Replace("{3}", productListSb.ToString())
                        .Replace("{4}", total.ToString());

            // Send email
            if (productUserVm.User is { Email: not null })
                await emailSender.SendEmailAsync(productUserVm.User.Email, subject, htmlBody);

            return RedirectToAction(nameof(InquiryConfirmation));
        
    }

    public IActionResult InquiryConfirmation()
    {
        HttpContext.Session.Clear();
        return View();
    }
    
    
    [HttpGet]
    public IActionResult SearchCities(string query)
    {
        var citiesResponse = addressService.GetCities(new CitiesGetRequest { FindByString = query });
        var cities = citiesResponse.Data.Select(city => city.Description).ToList();

        return Json(cities);
    }
    [HttpGet]
    public Task<IActionResult> GetWarehouses(string city, string query)
    {
        var warehousesResponse = addressService.GetWarehouses(city);
        var warehouses = warehousesResponse.Data
            .Select(warehouse => warehouse.Description)
            .Where(warehouse => string.IsNullOrEmpty(query) || warehouse.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Task.FromResult<IActionResult>(Json(warehouses));
    }
}