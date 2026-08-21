using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.AddSponsor
{
    public class IndexModel : PageModel
    {
		public WydarzenieSportowe WydarzenieSportowe { get; set; } = null!;
		public IList<string> NazwySponsorow { get; set; }
		public IList<string> NumeryBudynkowSponsorow { get; set; }
		[BindProperty]
		[Required(ErrorMessage = "Nazwa jest wymagana!")]
		public string Nazwa { get; set; } = string.Empty;
		[BindProperty]
		[Required(ErrorMessage = "Miejscowoœæ jest wymagana!")]
		public string Miejscowosc { get; set; } = string.Empty;
		[BindProperty]
		[Required(ErrorMessage = "Ulica jest wymagana!")]
		public string Ulica { get; set; } = string.Empty;
		[BindProperty]
		[Required(ErrorMessage = "Numer budynku jest wymagany!")]
		public string NumerBudynku { get; set; } = string.Empty;
		[BindProperty]
		[Required(ErrorMessage = "Kod pocztowy jest wymagany!")]
		public string KodPocztowy { get; set; } = string.Empty;
		public void OnGet(int id)
		{
			var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
			if (isLoggedIn != "true")
			{
				Response.Redirect("/Index");
			}

			HttpContext.Session.SetString("eventId", id.ToString());
			WydarzenieSportowe = new WydarzenieSportowe(id, "", "", "");

			string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();

					string getSportEvent = "SELECT Ogloszenie FROM WydarzenieSportowe WHERE IdWydarzenieSportowe=@id";

					using (SqlCommand command = new SqlCommand(getSportEvent, connection))
					{
						command.Parameters.AddWithValue("@id", id);

						WydarzenieSportowe.Ogloszenie = (string)command.ExecuteScalar();
					}

					string getSportEventNazwa = "SELECT Nazwa FROM WydarzenieSportowe WHERE IdWydarzenieSportowe=@id";

					using (SqlCommand command = new SqlCommand(getSportEventNazwa, connection))
					{
						command.Parameters.AddWithValue("@id", id);

						WydarzenieSportowe.Nazwa = (string)command.ExecuteScalar();
					}

					string getSportEventLogo = "SELECT Logo FROM WydarzenieSportowe WHERE IdWydarzenieSportowe=@id";

					using (SqlCommand command = new SqlCommand(getSportEventLogo, connection))
					{
						command.Parameters.AddWithValue("@id", id);

						WydarzenieSportowe.Logo = (string)command.ExecuteScalar();
					}

					connection.Close();
				}
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = ex.Message;
				Response.Redirect("/HelpingTheEvent?id=" + HttpContext.Session.GetString("id"));
			}
		}

		public IActionResult OnPost()
		{

			int id = int.Parse(HttpContext.Session.GetString("eventId"));

			WydarzenieSportowe = new WydarzenieSportowe(id, "", "", "");

			string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();

					string getSportEvent = "SELECT Ogloszenie FROM WydarzenieSportowe WHERE IdWydarzenieSportowe=@id";

					using (SqlCommand command = new SqlCommand(getSportEvent, connection))
					{
						command.Parameters.AddWithValue("@id", id);

						WydarzenieSportowe.Ogloszenie = (string)command.ExecuteScalar();
					}

					string getSportEventNazwa = "SELECT Nazwa FROM WydarzenieSportowe WHERE IdWydarzenieSportowe=@id";

					using (SqlCommand command = new SqlCommand(getSportEventNazwa, connection))
					{
						command.Parameters.AddWithValue("@id", id);

						WydarzenieSportowe.Nazwa = (string)command.ExecuteScalar();
					}

					string getSportEventLogo = "SELECT Logo FROM WydarzenieSportowe WHERE IdWydarzenieSportowe=@id";

					using (SqlCommand command = new SqlCommand(getSportEventLogo, connection))
					{
						command.Parameters.AddWithValue("@id", id);

						WydarzenieSportowe.Logo = (string)command.ExecuteScalar();
					}

					if (!ModelState.IsValid)
					{
						return Page();
					}

					string getSponsorsNames = "SELECT Nazwa FROM Sponsor";

					using (SqlCommand command = new SqlCommand(getSponsorsNames, connection))
					{
						using (SqlDataReader reader = command.ExecuteReader())
						{
							NazwySponsorow = new List<string>();
							string nazwa;
							while (reader.Read())
							{
								nazwa = reader.GetString(0);
								NazwySponsorow.Add(nazwa);
							}
						}
					}

					string getSponsorsBuildingNumbers = "SELECT NumerBudynku FROM Sponsor";

					using (SqlCommand command = new SqlCommand(getSponsorsBuildingNumbers, connection))
					{
						using (SqlDataReader reader = command.ExecuteReader())
						{
							NumeryBudynkowSponsorow = new List<string>();
							string numerBudynku;
							while (reader.Read())
							{
								numerBudynku = reader.GetString(0);
								NumeryBudynkowSponsorow.Add(numerBudynku);
							}
						}
					}

					if (NazwySponsorow.Contains(Nazwa))
					{
						TempData["ErrorMessage"] = "Sponsor o takiej nazwie ju¿ istnieje w bazie danych!";
						return RedirectToPage();
					}

					if (NumeryBudynkowSponsorow.Contains(NumerBudynku))
					{
						TempData["ErrorMessage"] = "Sponsor o takim numerze budynku ju¿ istnieje w bazie danych!";
						return RedirectToPage();
					}

					string addSponsor = "INSERT INTO Sponsor(Nazwa, Miejscowosc, Ulica, NumerBudynku, KodPocztowy) VALUES(@nazwa, @miejscowosc, @ulica, @numerBudynku, @kodPocztowy)";

					using (SqlCommand command = new SqlCommand(addSponsor, connection))
					{
						command.Parameters.AddWithValue("@nazwa", Nazwa);
						command.Parameters.AddWithValue("@miejscowosc", Miejscowosc);
						command.Parameters.AddWithValue("@ulica", Ulica);
						command.Parameters.AddWithValue("@numerBudynku", NumerBudynku);
						command.Parameters.AddWithValue("@kodPocztowy", KodPocztowy);

						command.ExecuteNonQuery();
					}

					connection.Close();

					TempData["SuccessMessage"] = "Uda³o Ci siê dodaæ sponsora!";
					return RedirectToPage("/HelpingTheEvent/Index", new { id = HttpContext.Session.GetString("eventId") });
				}
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = ex.Message;
				return RedirectToPage("/HelpingTheEvent/Index", new { id = HttpContext.Session.GetString("eventId") });
			}
		}
	}
}
