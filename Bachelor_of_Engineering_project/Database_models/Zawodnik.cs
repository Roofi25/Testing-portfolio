using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych
{
    public class Zawodnik
    {
        [Key] public int IdZawodnik { get; set; }
        [ForeignKey("Zawody")] public int IdZawody { get; set; }
        [ForeignKey("Osoba")] public int IdOsoba { get; set; }
    }
}
