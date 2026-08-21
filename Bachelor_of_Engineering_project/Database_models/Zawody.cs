using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych
{
    public class Zawody
    {
        [Key] public int IdZawody { get; set; }
        [ForeignKey("Dyscyplina")] public int IdDyscyplina { get; set; }
        [ForeignKey("WydarzenieSportowe")] public int IdWydarzenieSportowe { get; set; }
        [ForeignKey("ObiektSportowy")] public int IdObiektSportowy { get; set; }
        public DateTime Data { get; set; }

        public Zawody(int idZawody, DateTime data)
        {
            IdZawody = idZawody;
            Data = data;
        }
    }
}
