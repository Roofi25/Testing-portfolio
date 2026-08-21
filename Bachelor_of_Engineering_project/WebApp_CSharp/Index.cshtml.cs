using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            string? isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            if (isLoggedIn == "true")
            {
                Response.Redirect("/IndexForLoggedUser");
            }
            else
            {
                Response.Redirect("/IndexForNotLoggedUser");
            }
        }
    }
}
