using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.AddTheEvent
{
    public class IndexModel : PageModel
    {
        [BindProperty]
		[Required(ErrorMessage = "Nazwa jest wymagana!")]
		public string Nazwa { get; set; } = null!;
        [BindProperty]
		[Required(ErrorMessage = "Og³oszenie jest wymagane!")]
		public string Ogloszenie { get; set; } = null!;
        [BindProperty]
        [Range(1, 10, ErrorMessage = "Liczba zawodów musi byæ z przedzia³u od 1 do 10!")]
        public int LiczbaZawodow { get; set; }
        public void OnGet()
        {
			var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
			if (isLoggedIn != "true")
			{
				Response.Redirect("/Index");
			}
			else
			{
                var nazwaWydarzenia = HttpContext.Session.GetString("NazwaWydarzenia");
                var ogloszenieWydarzenia = HttpContext.Session.GetString("OgloszenieWydarzenia");
                var liczbaZawodowWydarzenia = HttpContext.Session.GetString("LiczbaZawodowWydarzenia");
				if(nazwaWydarzenia != null && ogloszenieWydarzenia != null && liczbaZawodowWydarzenia != null)
				{
					Nazwa = nazwaWydarzenia;
					Ogloszenie = ogloszenieWydarzenia;
					LiczbaZawodow = int.Parse(liczbaZawodowWydarzenia);
				}
            }
		}
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            HttpContext.Session.SetString("NazwaWydarzenia", Nazwa);
			HttpContext.Session.SetString("OgloszenieWydarzenia", Ogloszenie);
			HttpContext.Session.SetString("LiczbaZawodowWydarzenia", LiczbaZawodow.ToString());
			return RedirectToPage("/AddTheEvent2/Index");
		}
	}
}
