using System.ComponentModel.DataAnnotations;

namespace ElStore.Models;

public class DescriptionPC
{
    [Key]
    public int Id { get; set; }
    public int RAM { get; set; }
    public int ROM { get; set; }
    public string FrontCamera { get; set; }
    public string BackCamera { get; set; }
    public string Text { get; set; }
}