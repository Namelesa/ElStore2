using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Baroque.NovaPoshta.Client.Domain.Address;
using Baroque.NovaPoshta.Client.Services.Address;
using Data_Access.Repository.IRepository;
using Models;
using Models.ViewModel;
using Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.UI.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ElStore.Controllers;
public class CartController : Controller
{
    private readonly AddressService _addressService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IEmailSender _emailSender;
    private readonly IProductRepository _prodRepo;
    private readonly IUserRepository _userRepo;
    private readonly IOrderHeaderRepository _orderHeader;
    private readonly IOrderDetailsRepository _orderDetails;
    
    public CartController(AddressService addressService, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender,IProductRepository prodRepo, IUserRepository userRepo, IOrderHeaderRepository orderHeader, IOrderDetailsRepository orderDetails)
    {
        _addressService = addressService;
        _webHostEnvironment = webHostEnvironment;
        _emailSender = emailSender;
        _userRepo = userRepo;
        _prodRepo = prodRepo;
        _orderDetails = orderDetails;
        _orderHeader = orderHeader;
    }
    
    [BindProperty] private ProductUserVM ProductUserVm { get; set; }
    private static string _token = "test";

    //Index get
    public IActionResult Index()
    {
        List<ShoppingCart> shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart) ?? new List<ShoppingCart>();
        List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();

        var products = _prodRepo.GetAll(
            filter: p => prodInCart.Contains(p.Id),
            includeProperties: "Images");

        var productVMs = products.Select(product => new ProductVM
        {
            Product = product,
            Image = product.Images.Image,
            Count = shoppingCartList.FirstOrDefault(x => x.ProductId == product.Id)?.Count ?? 1
        }).ToList();

        double totalSum = productVMs.Sum(p => p.Product.Price * p.Count);
        ViewBag.TotalSum = totalSum;

        return View(productVMs);
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
            if (item != null)
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

                var products = _prodRepo.GetAll(
                    filter: p => prodInCart.Contains(p.Id),
                    includeProperties: "Images");

                var productVMs = products.Select(product => new ProductVM
                {
                    Product = product,
                    Image = product.Images.Image,
                    Count = shoppingCartList.FirstOrDefault(x => x.ProductId == product.Id)?.Count ?? 1
                }).ToList();
                
                double totalSum = productVMs.Sum(p => p.Product.Price * p.Count);
                ViewBag.TotalSum = totalSum;


                var user = _userRepo.FirstOrDefault(u => u.UserName == userName || u.NormalizedUserName == userName);
                
                ProductUserVm = new ProductUserVM()
                {
                    User = user,
                    ProductList = productVMs,
                    Total = totalSum,
                };
            }
        }
        return View(ProductUserVm);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [ActionName("Summary")]
    public async Task<IActionResult> SummaryPost(ProductUserVM productUserVm, string paymentMethod, string shippingMethod)
    {
        var claimUser = User.Identity as ClaimsIdentity;
        var claim = claimUser?.FindFirst(ClaimTypes.NameIdentifier);
        var pathToTemplate = Path.Combine(_webHostEnvironment.WebRootPath, "templates", "Inquiry.html");
        string htmlBody;
        string subject = "New Inquiry";

        using (StreamReader sr = System.IO.File.OpenText(pathToTemplate))
        {
            htmlBody = await sr.ReadToEndAsync();
        }
        
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
        
        
        htmlBody = htmlBody.Replace("{0}", productUserVm.FullName)
                .Replace("{1}", productUserVm.User.Email)
                .Replace("{2}", productUserVm.PhoneNumber)
                .Replace("{3}", productListSb.ToString())
                .Replace("{4}", total.ToString());
            
        if (productUserVm.User is { Email: not null })
            await _emailSender.SendEmailAsync(productUserVm.User.Email, subject, htmlBody);
        await _emailSender.SendEmailAsync(WC.AdminEmail, subject, htmlBody);
        
        if (claim != null)
        {
            OrderHeader orderHeader = new OrderHeader()
            {
                UserId = claim.Value,
                FullName = productUserVm.FullName,
                PhoneNumber = productUserVm.PhoneNumber,
                OrderDate = DateTime.UtcNow,
                PaymentType = paymentMethod,
                DeliveryType = shippingMethod,
                OrderStatus = WC.OrderStart,
                PaymentStatus = WC.PaymentStart,
                Total = total
            };
            if (orderHeader.DeliveryType == "nova-post")
            {
                orderHeader.City = productUserVm.City;
                orderHeader.Warehouse = productUserVm.NumberOffice;
            }
            else
            {
                orderHeader.City = WC.City;
                orderHeader.Warehouse = WC.Number;
            }

            if (paymentMethod == "card")
            {
                try
                {
                    productUserVm.PaymentToken = _token;
                    var paymentGatewayResponse = await ProcessPaymentWithGatewayAsync(productUserVm.PaymentToken);
                    if (paymentGatewayResponse.IsSuccess)
                    {
                        orderHeader.PaymentStatus = WC.PaymentEnd;
                    }
                    else
                    {
                        return BadRequest(new { success = false, message = "Payment failed" });
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }
            }
            
            _orderHeader.Add(orderHeader);
            _orderHeader.Save();
            
            foreach (var product in productUserVm.ProductList)
            {
                OrderDetails orderDetails = new OrderDetails()
                {
                    OrderHeaderId = orderHeader.Id,
                    ProductId = product.Product.Id,
                };
                _orderDetails.Add(orderDetails);
                
            }
            _orderDetails.Save();
        }
        
        return RedirectToAction(nameof(InquiryConfirmation));
        
    }

    [HttpPost]
    [Route("Cart/CheckPayment")]
    public async Task<IActionResult> CheckPayment([FromBody] JsonElement paymentData)
    {
        try
        {
            if (!paymentData.TryGetProperty("paymentMethodData", out JsonElement paymentMethodData))
            {
                return BadRequest("paymentMethodData is missing");
            }

            if (!paymentMethodData.TryGetProperty("tokenizationData", out JsonElement tokenizationData))
            {
                return BadRequest("tokenizationData is missing");
            }

            if (!tokenizationData.TryGetProperty("token", out JsonElement tokenElement))
            {
                return BadRequest("Payment token is missing");
            }

            var paymentToken = tokenElement.GetString();
            if (string.IsNullOrEmpty(paymentToken))
            {
                return BadRequest("Payment token is empty");
            }
            
            _token = paymentToken;
            
            var paymentGatewayResponse = await ProcessPaymentWithGatewayAsync(paymentToken);

            if (paymentGatewayResponse.IsSuccess)
            {
                return RedirectToAction(nameof(Summary));
            }
            else
            {
                return BadRequest(new { success = false, message = "Payment failed" });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
    
    private async Task<PaymentGatewayResponse> ProcessPaymentWithGatewayAsync(string paymentToken)
    {
        /*var httpClient = new HttpClient();
        var content = new StringContent(JsonSerializer.Serialize(new { token = paymentToken }), Encoding.UTF8, "application/json");
        */

        bool response = true;

        return new PaymentGatewayResponse { IsSuccess = response};
    }
    
    public IActionResult InquiryConfirmation()
    {
        HttpContext.Session.Clear();
        return View();
    }
    
    [HttpGet]
    public IActionResult SearchCities(string query)
    {
        var citiesResponse = _addressService.GetCities(new CitiesGetRequest { FindByString = query });
        var cities = citiesResponse.Data.Select(city => city.Description).ToList();

        return Json(cities);
    }
    [HttpGet]
    public Task<IActionResult> GetWarehouses(string city, string query)
    {
        var warehousesResponse = _addressService.GetWarehouses(city);
        var warehouses = warehousesResponse.Data
            .Select(warehouse => warehouse.Description)
            .Where(warehouse => string.IsNullOrEmpty(query) || warehouse.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Task.FromResult<IActionResult>(Json(warehouses));
    }
}