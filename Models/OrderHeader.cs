using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Models;

public class OrderHeader
{
    [Key]
    public int Id { get; set; }
    public string? UserId { get; set; }
    [ForeignKey("UserId")]
    [ValidateNever]
    public AllUser? User { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? Warehouse { get; set; }
    [Required]
    public string FullName { get; set; }
    
    public DateTime OrderDate { get; set; }
    public DateTime ShippingDate { get; set; }

    public double Total { get; set; }
    public string PaymentType { get; set; }
    public string DeliveryType { get; set; }
    public string OrderStatus { get; set; }
    public string PaymentStatus { get; set; }
    public DateTime PaymentDate { get; set; }
}