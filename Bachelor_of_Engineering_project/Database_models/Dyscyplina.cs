using System.ComponentModel.DataAnnotations;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych
{
    public class Dyscyplina
    {
        [Key] public int IdDyscyplina { get; set; }
        public string Nazwa { get; set; }
        public Dyscyplina(int idDyscyplina, string nazwa)
        {
            IdDyscyplina = idDyscyplina;
            Nazwa = nazwa;
        }
    }
}
