using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych
{
    public class WydarzenieSportowe
    {
        [Key] public int IdWydarzenieSportowe { get; set; }
        [ForeignKey("Osoba")] public int IdOrganizator { get; set; }
		public string Nazwa { get; set; }
		public string Ogloszenie { get; set; }
        public string Logo { get; set; }

        public WydarzenieSportowe(int idWydarzenieSportowe, string nazwa, string ogloszenie, string logo)
        {
            IdWydarzenieSportowe = idWydarzenieSportowe;
            Nazwa = nazwa;
            Ogloszenie = ogloszenie;
            Logo = logo;
        }
    }
}
