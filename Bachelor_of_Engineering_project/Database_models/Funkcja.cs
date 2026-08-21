using System.ComponentModel.DataAnnotations;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych
{
    public class Funkcja
    {
        [Key] public int IdFunkcja { get; set; }
        public string Nazwa { get; set; }
        public string Opis { get; set; }

        public Funkcja(int idFunkcja, string nazwa, string opis)
        {
            IdFunkcja = idFunkcja;
            Nazwa = nazwa;
            Opis = opis;
        }

    }
}
