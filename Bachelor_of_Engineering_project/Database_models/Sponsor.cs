using System.ComponentModel.DataAnnotations;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych
{
    public class Sponsor
    {
        [Key] public int IdSponsor { get; set; }
        public string Nazwa { get; set; }
        public string Miejscowosc { get; set; }
        public string Ulica { get; set; }
        public string NumerBudynku { get; set; }
        public string KodPocztowy { get; set; }
        public Sponsor(int idSponsor, string nazwa, string miejscowosc, string ulica, string numerBudynku, string kodPocztowy)
        {
            IdSponsor = idSponsor;
            Nazwa = nazwa;
            Miejscowosc = miejscowosc;
            Ulica = ulica;
            NumerBudynku = numerBudynku;
            KodPocztowy = kodPocztowy;
        }
    }
}
