using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.MorePersonInfo
{
    public class IndexModel : PageModel
    {
        public Osoba Osoba { get; set; }
        public void OnGet(int id)
        {
			var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
			if (isLoggedIn != "true")
			{
				Response.Redirect("/Index");
			}

			string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();

					string getOsoba = "SELECT IdOsoba, Imie, Nazwisko, Email, DataUrodzenia, NumerTelefonu FROM Osoba WHERE IdOsoba = @id";

					using(SqlCommand command = new SqlCommand(getOsoba, connection))
					{
						command.Parameters.AddWithValue("@id", id);
						int idOsoba;
						string imie;
						string nazwisko;
						string email;
						DateTime dataUrodzenia;
						string numerTelefonu;
						using(SqlDataReader reader = command.ExecuteReader())
						{
							while(reader.Read())
							{
								idOsoba = reader.GetInt32(0);
								imie = reader.GetString(1);
								nazwisko = reader.GetString(2);
								email = reader.GetString(3);
								dataUrodzenia = reader.GetDateTime(4);
								numerTelefonu = reader.GetString(5);
								Osoba = new Osoba(idOsoba, imie, nazwisko, email, dataUrodzenia, numerTelefonu);
							}
						}
					}

					connection.Close();
				}
			}
			catch(Exception ex)
			{
				TempData["ErrorMessage"] = ex.Message;
				Response.Redirect("/MoreInfo?id=" + HttpContext.Session.GetString("id"));
			}
		}
    }
}
