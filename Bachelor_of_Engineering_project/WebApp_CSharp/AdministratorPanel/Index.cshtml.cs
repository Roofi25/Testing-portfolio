using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.AdministratorPanel
{
    public class IndexModel : PageModel
    {
		public IList<Osoba>? UzytkownicyDoWyswietlenia { get; set; }
		bool IsAdmin(int idOsoba)
        {
            return idOsoba == 1;
        }
        public void OnGet()
        {
            HttpContext.Session.Remove("idUzytkownika");
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            if (isLoggedIn != "true")
            {
                Response.Redirect("/Index");
            }

            string? json = HttpContext.Session.GetString("LoggedUser");
            Osoba loggedUser = JsonSerializer.Deserialize<Osoba>(json);

            if (!IsAdmin(loggedUser.IdOsoba))
            {
                Response.Redirect("/Index");
            }

			string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

            try
            {
                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string getUsers = "SELECT DISTINCT O.IdOsoba, O.Imie, O.Nazwisko, O.Email, O.DataUrodzenia, O.NumerTelefonu FROM Osoba O LEFT JOIN OsobaOdpowiedzialna OO ON O.IdOsoba = OO.IdOsoba LEFT JOIN Zawodnik Z ON O.IdOsoba = Z.IdOsoba WHERE Z.IdOsoba IS NULL AND O.IdOsoba!=@id";

                    using (SqlCommand command = new SqlCommand(getUsers, connection))
                    {
                        command.Parameters.AddWithValue("@id", loggedUser.IdOsoba);
                        using(SqlDataReader reader = command.ExecuteReader())
                        {
                            UzytkownicyDoWyswietlenia = new List<Osoba>();
                            int idOsoba;
                            string imie;
                            string nazwisko;
                            string email;
                            DateTime dataUrodzenia;
                            string numerTelefonu;
                            while(reader.Read())
                            {
                                idOsoba = reader.GetInt32(0);
                                imie = reader.GetString(1);
                                nazwisko = reader.GetString(2);
                                email = reader.GetString(3);
                                dataUrodzenia = reader.GetDateTime(4);
                                numerTelefonu = reader.GetString(5);
                                UzytkownicyDoWyswietlenia.Add(new Osoba(idOsoba, imie, nazwisko, email, dataUrodzenia, numerTelefonu));
                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                Response.Redirect("/Index");
            }
		}
    }
}
