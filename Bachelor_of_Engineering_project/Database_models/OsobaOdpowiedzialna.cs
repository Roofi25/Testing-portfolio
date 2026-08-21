using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych
{
    public class OsobaOdpowiedzialna
    {
        [Key] public int IdOsobaOdpowiedzialna { get; set; }
        [ForeignKey("Osoba")] public int IdOsoba { get; set; }
        [ForeignKey("Zawody")] public int IdZawody { get; set; }
        [ForeignKey("Funkcja")] public int IdFunkcja { get; set; }
    }
}
