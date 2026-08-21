using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych
{
    public class Poczestunek
    {
        [Key] public int IdPoczestunek { get; set; }
        [ForeignKey("LokalGastronomiczny")] public int IdLokalGastronomiczny { get; set; }
        [ForeignKey("Zawody")] public int IdZawody { get; set; }
    }
}
