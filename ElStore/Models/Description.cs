using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ElStore.Models;

public class Description
{
    [Key]
    public int Id { get; set; }
    
    public string RAM { get; set; }
    public string ROM { get; set; }
    public string Text { get; set; }
}