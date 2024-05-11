using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElStore.Models;

public class DescriptionPC
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int RAM { get; set; }
    public float Display { get; set; }
    public int ROM { get; set; }
    public string FrontCamera { get; set; }
    public string BackCamera { get; set; }
    public string Text { get; set; }
}