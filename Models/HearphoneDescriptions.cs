using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

public class HearphoneDescriptions
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public double SpeakerSize { get; set; }
    public string TypeConnections { get; set; }
    public string Design { get; set; }
    public string Text { get; set; }
}