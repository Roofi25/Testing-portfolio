using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.ConfirmUserDeletion
{
    public class IndexModel : PageModel
    {
        bool IsAdmin(int idOsoba)
        {
            return idOsoba == 1;
        }
        public void OnGet(int id)
        {
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            if (isLoggedIn != "true")
            {
                Response.Redirect("/Index");
            }

            //pobieramy jsona z sesji
            string? json = HttpContext.Session.GetString("LoggedUser");
            //deserializujemy jsona do obiektu o nazwie loggedUser, ktory bedzie zawiera³ informacje o aktualnie zalogowanym u¿ytkowniku
            Osoba loggedUser = JsonSerializer.Deserialize<Osoba>(json);

            if (!IsAdmin(loggedUser.IdOsoba))
            {
                Response.Redirect("/Index");
            }

            HttpContext.Session.SetInt32("idUzytkownika", id);
        }

        public void OnPost()
        {
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            if (isLoggedIn != "true")
            {
                Response.Redirect("/Index");
            }

            //pobieramy jsona z sesji
            string? json = HttpContext.Session.GetString("LoggedUser");
            //deserializujemy jsona do obiektu o nazwie loggedUser, ktory bedzie zawiera³ informacje o aktualnie zalogowanym u¿ytkowniku
            Osoba loggedUser = JsonSerializer.Deserialize<Osoba>(json);

            if (!IsAdmin(loggedUser.IdOsoba))
            {
                Response.Redirect("/Index");
            }
            try
            {
                string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string deleteUserFromOsobaOdpowiedzialna = "DELETE FROM OsobaOdpowiedzialna WHERE IdOsoba=@id";

                    using (SqlCommand command = new SqlCommand(deleteUserFromOsobaOdpowiedzialna, connection))
                    {
                        command.Parameters.AddWithValue("@id", HttpContext.Session.GetInt32("idUzytkownika"));

                        command.ExecuteNonQuery();
                    }


                    string deleteUserFromLoginData = "DELETE FROM LoginData WHERE IdOsoba=@id";

                    using (SqlCommand command = new SqlCommand(deleteUserFromLoginData, connection))
                    {
                        command.Parameters.AddWithValue("@id", HttpContext.Session.GetInt32("idUzytkownika"));

                        command.ExecuteNonQuery();
                    }

                    string deleteUserFromOsoba = "DELETE FROM Osoba WHERE IdOsoba=@id";

                    using (SqlCommand command = new SqlCommand(deleteUserFromOsoba, connection))
                    {
                        command.Parameters.AddWithValue("@id", HttpContext.Session.GetInt32("idUzytkownika"));

                        command.ExecuteNonQuery();
                    }
                }
                TempData["SuccessMessage"] = "Pomyœlnie usuniêto u¿ytkownika!";
                TempData.Keep();
                Response.Redirect("/AdministratorPanel");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                Response.Redirect("/AdministratorPanel");
            }
        }
    }
}
