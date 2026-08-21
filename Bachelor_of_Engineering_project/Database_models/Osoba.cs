using System.ComponentModel.DataAnnotations;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych
{
    public class Osoba
    {
        [Key] public int IdOsoba { get; set; }
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public string Email { get; set; }
        public DateTime DataUrodzenia { get; set; }
        public string NumerTelefonu { get; set; }
        public Osoba()
        {
            IdOsoba = 0;
            Imie = string.Empty;
            Nazwisko = string.Empty;
            Email = string.Empty;
            DataUrodzenia = DateTime.MinValue;
            NumerTelefonu = string.Empty;
        }
        public Osoba(int idOsoba, string imie, string nazwisko, string email, DateTime dataUrodzenia, string numerTelefonu)
        {
            IdOsoba = idOsoba;
            Imie = imie;
            Nazwisko = nazwisko;
            Email = email;
            DataUrodzenia = dataUrodzenia;
            NumerTelefonu = numerTelefonu;
        }
    }
}
