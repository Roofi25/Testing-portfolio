using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych
{
    public class SponsorZawodow
    {
        [Key] public int IdSponsorZawodow { get; set; }
        [ForeignKey("Zawody")] public int IdZawody { get; set; }
        [ForeignKey("Sponsor")] public int IdSponsor { get; set; }
    }
}
