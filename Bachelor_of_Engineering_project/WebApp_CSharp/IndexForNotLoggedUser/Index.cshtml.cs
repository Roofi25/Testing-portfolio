using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages
{
    public class IndexForNotLoggedUserModel : PageModel
    {
        public string successMessage;
        public void OnGet()
        {
			var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
			if (isLoggedIn == "true")
			{
				Response.Redirect("/IndexForLoggedUser");
			}
			if (TempData["SuccessMessage"] != null)
            {
                successMessage = TempData["SuccessMessage"].ToString();
                TempData["SuccessMessage"] = null;
            }
        }
    }
}
