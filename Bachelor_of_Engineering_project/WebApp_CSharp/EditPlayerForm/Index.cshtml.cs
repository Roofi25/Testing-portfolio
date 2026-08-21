using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.Inne;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.AddHelpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.EditPlayerForm
{
    public class IndexModel : PageModel
    {
		public WydarzenieSportowe? WydarzenieSportowe { get; set; }
		public IList<DyscyplinaOrazData>? DyscyplinyOrazDaty { get; set; }
		public IList<Zawody> Zawody { get; set; }
		public Osoba? Organizator { get; set; }
		public int IloscDyscyplin { get; set; }
		[BindProperty]
		public Osoba WybranyZawodnikDoEdycji { get; set; }
		public List<int> WybraneZawody { get; set; } = new List<int>();
		public void OnGet(int id, int idZawodnika, string wybraneZawody)
		{
			var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
			if (isLoggedIn != "true")
			{
				Response.Redirect("/Index");
			}

			HttpContext.Session.SetString("id", id.ToString());
			WydarzenieSportowe = new WydarzenieSportowe(id, "", "", "");

			if(!wybraneZawody.IsNullOrEmpty())
			{
				WybraneZawody = wybraneZawody.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
			}

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

                    string getDyscyplinyOrazDaty = "SELECT Dyscyplina.IdDyscyplina, Dyscyplina.Nazwa, Zawody.Data FROM Dyscyplina INNER JOIN Zawody ON Dyscyplina.IdDyscyplina = Zawody.IdDyscyplina INNER JOIN WydarzenieSportowe ON Zawody.IdWydarzenieSportowe = WydarzenieSportowe.IdWydarzenieSportowe WHERE WydarzenieSportowe.IdWydarzenieSportowe = @id";

					using (SqlCommand command = new SqlCommand(getDyscyplinyOrazDaty, connection))
					{
						command.Parameters.AddWithValue("@id", id);

						using (SqlDataReader reader = command.ExecuteReader())
						{
							DyscyplinyOrazDaty = new List<DyscyplinaOrazData>();
							int IdDyscyplina;
							string nazwaDyscypliny;
							DateTime dataZawodow;
							while (reader.Read())
							{
								IdDyscyplina = reader.GetInt32(0);
								nazwaDyscypliny = reader.GetString(1);
								dataZawodow = reader.GetDateTime(2);
								DyscyplinyOrazDaty.Add(new DyscyplinaOrazData(IdDyscyplina, nazwaDyscypliny, dataZawodow)); //wykorzystanie konstruktora klasy ni¿ej
							}
						}
					}

					string getZawodyForSportEvent = "SELECT IdZawody, Data FROM Zawody WHERE IdWydarzenieSportowe=@id";

					using (SqlCommand command = new SqlCommand(getZawodyForSportEvent, connection))
					{
						command.Parameters.AddWithValue("@id", id);

						using (SqlDataReader reader = command.ExecuteReader())
						{
							Zawody = new List<Zawody>();
							int idZawody;
							int idDyscyplina;
							int idWydarzenieSportowe;
							int? idObiektSportowy;
							DateTime data;
							while (reader.Read())
							{
								idZawody = reader.GetInt32(0);
								data = reader.GetDateTime(1);
								Zawody.Add(new Zawody(idZawody, data));
							}
						}
					}

					string getOrganisatorId = "SELECT IdOrganizator from WydarzenieSportowe WHERE IdWydarzenieSportowe = @id";

					using (SqlCommand command = new SqlCommand(getOrganisatorId, connection))
					{
						command.Parameters.AddWithValue("@id", id);

						string? organisatorId = command.ExecuteScalar().ToString();

						if (organisatorId == null)
						{
							organisatorId = "";
						}

						HttpContext.Session.SetString("organisatorId", organisatorId);

						if (organisatorId != null)
						{
							string getOrganisator = "SELECT IdOsoba, Imie, Nazwisko, Email, DataUrodzenia, NumerTelefonu from Osoba WHERE IdOsoba = @id";

							using (SqlCommand _command = new SqlCommand(getOrganisator, connection))
							{
								_command.Parameters.AddWithValue("@id", organisatorId);

								using (SqlDataReader reader = _command.ExecuteReader())
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
										Organizator = new Osoba(idOsoba, imie, nazwisko, email, dataUrodzenia, numerTelefonu);
									}
								}
							}
						}
					}

					string getIloscDyscyplin = "SELECT COUNT(*) FROM Zawody INNER JOIN WydarzenieSportowe ON Zawody.IdWydarzenieSportowe = WydarzenieSportowe.IdWydarzenieSportowe WHERE Zawody.IdWydarzenieSportowe = @id";

					using (SqlCommand command = new SqlCommand(getIloscDyscyplin, connection))
					{
						command.Parameters.AddWithValue("@id", id);
						IloscDyscyplin = (int)command.ExecuteScalar();
					}

					string getPickedPlayerData = "SELECT Imie, Nazwisko, Email, DataUrodzenia, NumerTelefonu FROM Osoba WHERE IdOsoba=@id";

					using(SqlCommand command = new SqlCommand(getPickedPlayerData, connection))
					{
						command.Parameters.AddWithValue("@id", idZawodnika);

						using(SqlDataReader reader = command.ExecuteReader())
						{
							string imie;
							string nazwisko;
							string email;
							DateTime dataUrodzenia;
							string numerTelefonu;
							while(reader.Read())
							{
								imie = reader.GetString(0);
								nazwisko = reader.GetString(1);
								email = reader.GetString(2);
								dataUrodzenia = reader.GetDateTime(3);
								numerTelefonu = reader.GetString(4);
								WybranyZawodnikDoEdycji = new Osoba(idZawodnika, imie, nazwisko, email, dataUrodzenia, numerTelefonu);
							}
						}
					}

				}
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = ex.Message;
				Response.Redirect("/Index");
			}
		}

		public void OnPost(List<int> WybraneZawody)
		{
			try
			{
				var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
				if (isLoggedIn != "true")
				{
					Response.Redirect("/Index");
				}

				int idWydarzenia = int.Parse(HttpContext.Session.GetString("id"));
				int idOsoby = int.Parse(Request.Form["idOsoba"]);

				string imie = Request.Form["imie"];
				string nazwisko = Request.Form["nazwisko"];
				string email = Request.Form["email"];
				if (DateTime.TryParse(Request.Form["dataUrodzenia"], out DateTime dataUrodzenia)) { }
				else
				{
					TempData["ErrorMessage"] = "Podano nieprawid³owy format daty!";
					TempData.Keep();
					Response.Redirect("/HelpingTheEvent?id=" + HttpContext.Session.GetInt32("idWydarzenieSportowe").ToString());
				}
				string numerTelefonu = Request.Form["numerTelefonu"];

				if (imie.IsNullOrEmpty() || nazwisko.IsNullOrEmpty() || email.IsNullOrEmpty() || numerTelefonu.IsNullOrEmpty())
				{
					TempData["ErrorMessage"] = "Wszystkie pola musz¹ zostaæ wype³nione!";
					TempData.Keep();
					Response.Redirect("/HelpingTheEvent?id=" + HttpContext.Session.GetInt32("idWydarzenieSportowe").ToString());
				}

				if (WybraneZawody == null || !WybraneZawody.Any())
				{
					TempData["ErrorMessage"] = "Zawodnik musi byæ zapisany przynajmniej na jedne zawody!";
					TempData.Keep();
					Response.Redirect("/HelpingTheEvent?id=" + HttpContext.Session.GetInt32("idWydarzenieSportowe").ToString());
				}

				string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

				using(SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();

					string updatePlayerCredentials = "UPDATE Osoba SET Imie=@imie, Nazwisko=@nazwisko, Email=@email, DataUrodzenia=@dataUrodzenia, NumerTelefonu=@numerTelefonu WHERE IdOsoba=@id";

					using (SqlCommand command = new SqlCommand(updatePlayerCredentials, connection))
					{
						command.Parameters.AddWithValue("@imie", imie);
						command.Parameters.AddWithValue("@nazwisko", nazwisko);
						command.Parameters.AddWithValue("@email", email);
						command.Parameters.AddWithValue("@dataUrodzenia", dataUrodzenia);
						command.Parameters.AddWithValue("@numerTelefonu", numerTelefonu);
						command.Parameters.AddWithValue("@id", idOsoby);

						command.ExecuteNonQuery();
					}

					string deletePlayerCurrentRecords = "DELETE z FROM Zawodnik z INNER JOIN Zawody zaw ON z.IdZawody = zaw.IdZawody INNER JOIN WydarzenieSportowe ws ON zaw.IdWydarzenieSportowe = ws.IdWydarzenieSportowe WHERE z.IdOsoba = @idOsoby AND ws.IdWydarzenieSportowe = @idWydarzenia";

					using (SqlCommand command = new SqlCommand(deletePlayerCurrentRecords, connection))
					{
						command.Parameters.AddWithValue("@idOsoby", idOsoby);
						command.Parameters.AddWithValue("@idWydarzenia", idWydarzenia);

						command.ExecuteNonQuery();
					}

					string addUpdatedPlayerRecords = "INSERT INTO Zawodnik(IdZawody, IdOsoba) VALUES(@idZawodow, @idOsoby)";

					for (int i = 0; i < WybraneZawody.Count; i++)
					{
						using (SqlCommand command = new SqlCommand(addUpdatedPlayerRecords, connection))
						{
							command.Parameters.AddWithValue("@idZawodow", WybraneZawody.ElementAt(i));
							command.Parameters.AddWithValue("@idOsoby", idOsoby);
							command.ExecuteNonQuery();
						}
					}

					WydarzenieSportowe = new WydarzenieSportowe(idWydarzenia, "", "", "");

					string getSportEvent = "SELECT Ogloszenie FROM WydarzenieSportowe WHERE IdWydarzenieSportowe=@id";

					using (SqlCommand command = new SqlCommand(getSportEvent, connection))
					{
						command.Parameters.AddWithValue("@id", idWydarzenia);

						WydarzenieSportowe.Ogloszenie = (string)command.ExecuteScalar();

					}

					string getSportEventNazwa = "SELECT Nazwa FROM WydarzenieSportowe WHERE IdWydarzenieSportowe=@id";

					using (SqlCommand command = new SqlCommand(getSportEventNazwa, connection))
					{
						command.Parameters.AddWithValue("@id", idWydarzenia);

						WydarzenieSportowe.Nazwa = (string)command.ExecuteScalar();
					}

					string getSportEventLogo = "SELECT Logo FROM WydarzenieSportowe WHERE IdWydarzenieSportowe=@id";

					using (SqlCommand command = new SqlCommand(getSportEventLogo, connection))
					{
						command.Parameters.AddWithValue("@id", idWydarzenia);

						WydarzenieSportowe.Logo = (string)command.ExecuteScalar();
					}

					connection.Close();

					TempData["SuccessMessage"] = "Pomyœlnie edytowano zawodnika!";
					TempData.Keep();
					Response.Redirect("/EditPlayers?id=" + HttpContext.Session.GetString("id"));
				}
			}
			catch(Exception ex)
			{
				TempData["ErrorMessage"] = ex.Message;
				Response.Redirect("/EditPlayers?id=" + HttpContext.Session.GetString("id"));
			}
		}
    }
}
