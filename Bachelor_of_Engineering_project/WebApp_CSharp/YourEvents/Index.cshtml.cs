using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages
{
    public class YourEventsModel : PageModel
    {
		public IList<WydarzenieSportowe>? TwojeWydarzeniaSportowe { get; set; }
		public IList<WydarzenieSportowe>? WydarzeniaSportoweKtorePomagaszOrganizowac { get; set; }
		public void OnGet()
        {
			var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
			if (isLoggedIn != "true")
			{
				Response.Redirect("/Index");
			}

			string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

			try
			{
				using(SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();

					string getMySportEvents = "SELECT DISTINCT WydarzenieSportowe.IdWydarzenieSportowe, Nazwa, Ogloszenie, Logo FROM WydarzenieSportowe INNER JOIN Zawody ON WydarzenieSportowe.IdWydarzenieSportowe = Zawody.IdWydarzenieSportowe WHERE WydarzenieSportowe.IdOrganizator = @id AND Zawody.Data > @data";

					using(SqlCommand command = new SqlCommand(getMySportEvents, connection))
					{
						//pobieramy jsona z sesji
						string json = HttpContext.Session.GetString("LoggedUser");
						//deserializujemy jsona do obiektu o nazwie loggedUser, ktory bedzie zawiera³ informacje o aktualnie zalogowanym u¿ytkowniku
						Osoba loggedUser = JsonSerializer.Deserialize<Osoba>(json);

						command.Parameters.AddWithValue("@id", loggedUser.IdOsoba);
						command.Parameters.AddWithValue("@data", DateTime.Now);

						using(SqlDataReader reader = command.ExecuteReader())
						{
							TwojeWydarzeniaSportowe = new List<WydarzenieSportowe>();
							int idWydarzenia;
							string nazwa;
							string ogloszenie;
							string logo;

							while (reader.Read())
							{
								idWydarzenia = reader.GetInt32(0);
								nazwa = reader.GetString(1);
								ogloszenie = reader.GetString(2);
								logo = reader.GetString(3);
								TwojeWydarzeniaSportowe.Add(new WydarzenieSportowe(idWydarzenia, nazwa, ogloszenie, logo)); //wykorzystanie konstruktora
							}
						}
					}

					string getSportEventsThatYouHelpWith = "SELECT DISTINCT WydarzenieSportowe.IdWydarzenieSportowe, WydarzenieSportowe.Nazwa, WydarzenieSportowe.Ogloszenie, WydarzenieSportowe.Logo FROM WydarzenieSportowe INNER JOIN OsobaOdpowiedzialna ON WydarzenieSportowe.IdWydarzenieSportowe = OsobaOdpowiedzialna.IdWydarzenieSportowe WHERE OsobaOdpowiedzialna.IdOsoba = @id";

					using(SqlCommand command = new SqlCommand(getSportEventsThatYouHelpWith, connection))
					{
						//pobieramy jsona z sesji
						string json = HttpContext.Session.GetString("LoggedUser");
						//deserializujemy jsona do obiektu o nazwie loggedUser, ktory bedzie zawiera³ informacje o aktualnie zalogowanym u¿ytkowniku
						Osoba loggedUser = JsonSerializer.Deserialize<Osoba>(json);

						command.Parameters.AddWithValue("@id", loggedUser.IdOsoba);

						using(SqlDataReader reader = command.ExecuteReader())
						{
							WydarzeniaSportoweKtorePomagaszOrganizowac = new List<WydarzenieSportowe>();
							int idWydarzenia;
							string nazwa;
							string ogloszenie;
							string logo;
							while (reader.Read())
							{
								idWydarzenia = reader.GetInt32(0);
								nazwa = reader.GetString(1);
								ogloszenie = reader.GetString(2);
								logo = reader.GetString(3);
								WydarzeniaSportoweKtorePomagaszOrganizowac.Add(new WydarzenieSportowe(idWydarzenia, nazwa, ogloszenie, logo)); //wykorzystanie konstruktora
							}
						}
					}

					connection.Close();
				}
			}
			catch(Exception ex)
			{
				TempData["ErrorMessage"] = ex.Message;
				return;
			}
		}
    }
}
