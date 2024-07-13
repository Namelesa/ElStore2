using Data_Access.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models.ViewModel;

namespace ElStore.Controllers;

public class OrderController : Controller
{
    private readonly IOrderHeaderRepository _orderHRepo;
    private readonly IOrderDetailsRepository _orderDRepo;
    private readonly IProductRepository _prodRepo;
    private readonly IImageRepository _image;
    private readonly IDescriptionPcRepository _descriptionPc;
    private readonly IHeadphoneDescriptionRepository _headphoneDescription;

    public OrderController(IOrderHeaderRepository orderHRepo, IOrderDetailsRepository orderDRepo, IProductRepository prodRepo, IImageRepository image, IDescriptionPcRepository descriptionPc, IHeadphoneDescriptionRepository headphoneDescription)
    {
        _orderHRepo = orderHRepo;
        _orderDRepo = orderDRepo;
        _prodRepo = prodRepo;
        _image = image;
        _descriptionPc = descriptionPc;
        _headphoneDescription = headphoneDescription;
    }

    public IActionResult Index()
    {
        OrderVm orderVm = new OrderVm()
        {
            OrderHeader = _orderHRepo.GetAll(),
            OrderDetails = _orderDRepo.GetAll(),
        };
        
        return View(orderVm);
    }

    public IActionResult Details(int id)
    {
        var orderDetailsList = _orderDRepo.GetAll(u => u.OrderHeaderId == id);
        var productIds = orderDetailsList.Select(od => od.ProductId).ToList();
        var products = _prodRepo.GetAll(p => productIds.Contains(p.Id)).ToList();

        foreach (var product in products)
        {
            product.Images = _image.GetAll(pi => pi.Id == product.ImageId).FirstOrDefault();
            product.DescriptionPC = _descriptionPc.FirstOrDefault(d => d.Id == product.DescriptionPCId);
            product.HearphoneDescriptions = _headphoneDescription.FirstOrDefault(d => d.Id == product.DescriptionHId);
        }

        OrderDetailsVm orderDetailsVm = new OrderDetailsVm()
        {
            OrderHeader = _orderHRepo.FirstOrDefault(u => u.Id == id),
            OrderDetailsList = orderDetailsList,
            ProductList = products
        };

        return View(orderDetailsVm);
    }
    
    [HttpPost]
    public IActionResult Remove(int orderId, int productId)
    {
        
        var orderDetail = _orderDRepo.FirstOrDefault(od => od.OrderHeaderId == orderId && od.ProductId == productId);
        
        _orderDRepo.Remove(orderDetail);
        _orderDRepo.Save();
        
        var remainingOrderDetails = _orderDRepo.GetAll(od => od.OrderHeaderId == orderId);

        if (!remainingOrderDetails.Any())
        {
            var orderToRemove = _orderHRepo.FirstOrDefault(o => o.Id == orderId);
            
            _orderHRepo.Remove(orderToRemove);
            _orderHRepo.Save();
                
            return RedirectToAction(nameof(Index));
        }
        
        return RedirectToAction(nameof(Details), new { id = orderId });
    }
    
    public IActionResult CancelOrder(int id)
    {
        var header = _orderHRepo.FirstOrDefault(u => u.Id == id);
        var details = _orderDRepo.GetAll(u => u.OrderHeaderId == id);
        
        _orderHRepo.Remove(header);

        foreach (var item in details)
        {
            _orderDRepo.Remove(item);
        }
        
        _orderHRepo.Save();
        _orderDRepo.Save();
        
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public IActionResult Update(int id, string? fullName, string? phoneNumber, string? paymentStatus, string? deliveryStatus, string? warehouse, string? city)
    {
        var orderHeader = _orderHRepo.FirstOrDefault(o => o.Id == id);

        orderHeader.FullName = fullName ?? orderHeader.FullName;
        orderHeader.PhoneNumber = phoneNumber ?? orderHeader.PhoneNumber;
        orderHeader.PaymentStatus = paymentStatus ?? orderHeader.PaymentStatus;
        orderHeader.OrderStatus = deliveryStatus ?? orderHeader.OrderStatus;
        orderHeader.Warehouse = warehouse ?? orderHeader.Warehouse;
        orderHeader.City = city ?? orderHeader.City;
        
        orderHeader.OrderDate = DateTime.UtcNow;
            
        _orderHRepo.Update(orderHeader);
        _orderHRepo.Save();
        

        return RedirectToAction(nameof(Details), new { id });
    }
    #region API CALLS
    [HttpGet]
    public IActionResult GetList()
    {
        return Json(new { data=_orderHRepo.GetAll() });
    }
    #endregion
}