using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages
{
    public class LogoutModel : PageModel
    {
        public void OnGet()
        {
			HttpContext.Session.SetString("IsLoggedIn", "");
			HttpContext.Session.SetString("LoggedUser", "");
			TempData["SuccessMessage"] = "Wylogowano pomyœlnie!";
			Response.Redirect("/");
		}
    }
}
