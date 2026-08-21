using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.EditUserForm
{
    public class IndexModel : PageModel
    {
		[BindProperty]
		public Osoba WybranyUzytkownikDoEdycji { get; set; }
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

			string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

			try
			{
				using(SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();

					string getPickedUserData = "SELECT IdOsoba, Imie, Nazwisko, Email, DataUrodzenia, NumerTelefonu FROM Osoba WHERE IdOsoba=@id";

					using (SqlCommand command = new SqlCommand(getPickedUserData, connection))
					{
						command.Parameters.AddWithValue("@id", id);

						using (SqlDataReader reader = command.ExecuteReader())
						{
							int idOsoba;
							string imie;
							string nazwisko;
							string email;
							DateTime dataUrodzenia;
							string numerTelefonu;
							while (reader.Read())
							{
								idOsoba = reader.GetInt32(0);
								imie = reader.GetString(1);
								nazwisko = reader.GetString(2);
								email = reader.GetString(3);
								dataUrodzenia = reader.GetDateTime(4);
								numerTelefonu = reader.GetString(5);
								WybranyUzytkownikDoEdycji = new Osoba(idOsoba, imie, nazwisko, email, dataUrodzenia, numerTelefonu);
							}
						}
					}
				}
			}
			catch(Exception ex)
			{
				TempData["ErrorMessage"] = ex.Message;
				Response.Redirect("/AdministratorPanel");
			}
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

			string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

			try
			{
				int idOsoby = int.Parse(Request.Form["idOsoba"]);

				string imie = Request.Form["imie"];
				string nazwisko = Request.Form["nazwisko"];
				string email = Request.Form["email"];
				if (DateTime.TryParse(Request.Form["dataUrodzenia"], out DateTime dataUrodzenia)) { }
				else
				{
					TempData["ErrorMessage"] = "Podano nieprawid³owy format daty!";
					TempData.Keep();
					Response.Redirect("/AdministratorPanel");
				}
				string numerTelefonu = Request.Form["numerTelefonu"];

				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();

					string updateUserCredentials = "UPDATE Osoba SET Imie=@imie, Nazwisko=@nazwisko, Email=@email, DataUrodzenia=@dataUrodzenia, NumerTelefonu=@numerTelefonu WHERE IdOsoba=@id";

					using(SqlCommand command = new SqlCommand(updateUserCredentials, connection))
					{
						command.Parameters.AddWithValue("@imie", imie);
						command.Parameters.AddWithValue("@nazwisko", nazwisko);
						command.Parameters.AddWithValue("@email", email);
						command.Parameters.AddWithValue("@dataUrodzenia", dataUrodzenia);
						command.Parameters.AddWithValue("@numerTelefonu", numerTelefonu);
						command.Parameters.AddWithValue("@id", idOsoby);

						command.ExecuteNonQuery();
					}
				}

				TempData["SuccessMessage"] = "Pomyœlnie edytowano u¿ytkownika!";
				TempData.Keep();
				Response.Redirect("/AdministratorPanel");
			}
			catch(Exception ex)
			{
				TempData["ErrorMessage"] = ex.Message;
				Response.Redirect("/AdministratorPanel");
			}
		}
    }
}
