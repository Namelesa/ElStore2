using System.ComponentModel.DataAnnotations;

namespace ElStore.Models;

public class HearphoneDescriptions
{
    [Key]
    public int Id { get; set; }
    public double speaker { get; set; }
    public string TypeConnections { get; set; }
    public string Design { get; set; }
    public string Text { get; set; }
}