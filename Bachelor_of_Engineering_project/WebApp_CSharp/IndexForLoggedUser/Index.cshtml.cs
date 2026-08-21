using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages
{
    public class IndexForLoggedUserModel : PageModel
    {
        public string successMessage = "";
        public void OnGet()
        {
            HttpContext.Session.Remove("NazwaWydarzenia");
			HttpContext.Session.Remove("OgloszenieWydarzenia");
			HttpContext.Session.Remove("LiczbaZawodowWydarzenia");
			var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            if (isLoggedIn != "true")
            {
                Response.Redirect("/Index");
            }
            if (TempData["SuccessMessage"] != null)
            {
                successMessage = TempData["SuccessMessage"].ToString();
                TempData["SuccessMessage"] = null;
            }
        }
    }
}
