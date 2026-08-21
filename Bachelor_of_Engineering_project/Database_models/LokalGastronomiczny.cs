using System.ComponentModel.DataAnnotations;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych
{
    public class LokalGastronomiczny
    {
        [Key] public int IdLokalGastronomiczny { get; set; }
        public string Nazwa { get; set; }
        public string Miejscowosc { get; set; }
        public string Ulica { get; set; }
        public string NumerBudynku { get; set; }
        public string KodPocztowy { get; set; }

        public LokalGastronomiczny(int idLokalGastronomiczny, string nazwa, string miejscowosc, string ulica, string numerBudynku, string kodPocztowy)
        {
            IdLokalGastronomiczny = idLokalGastronomiczny;
            Nazwa = nazwa;
            Miejscowosc = miejscowosc;
            Ulica = ulica;
            NumerBudynku = numerBudynku;
            KodPocztowy = kodPocztowy;
        }
    }
}
