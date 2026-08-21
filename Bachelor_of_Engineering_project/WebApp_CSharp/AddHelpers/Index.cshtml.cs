using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.Inne;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.AddHelpers
{
    public class AddHelpersModel : PageModel
    {
		public WydarzenieSportowe? WydarzenieSportowe { get; set; }
		public IList<DyscyplinaOrazData>? DyscyplinyOrazDaty { get; set; }
		public Osoba? Organizator { get; set; }
		public IList<Osoba>? MozliwiPomocnicy { get; set; }

		//bindujemy w³aœciwoœci z modelu (cshtml), które bêd¹ zawiera³y id osób
		//wybranych przez organizatora wydarzenia sportowego
		[BindProperty]
		public int? WybranaOsobaDoObiektowSportowych { get; set; }
		[BindProperty]
		public int? WybranaOsobaDoLokaliGastronomicznych { get; set; }
		[BindProperty]
		public int? WybranaOsobaDoSposnorow { get; set; }
		[BindProperty]
		public int? WybranaOsobaDoZawodnikow { get; set; }
		public AktualnyPomocnik? PomocnikDoObiektowSportowych { get; set; }
		public AktualnyPomocnik? PomocnikDoLokaliGastronomicznych { get; set; }
		public AktualnyPomocnik? PomocnikDoSponsorow { get; set; }
		public AktualnyPomocnik? PomocnikDoZawodnikow { get; set; }
		public void OnGet(int id)
        {
			var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
			if (isLoggedIn != "true")
			{
				Response.Redirect("/Index");
			}

			HttpContext.Session.SetString("id", id.ToString());
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

					using(SqlCommand command = new SqlCommand(getSportEventNazwa, connection))
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

					//tworzenie listy mo¿liwych pomocników
					string getPossibleHelpers = "SELECT DISTINCT O.IdOsoba, O.Imie, O.Nazwisko, O.Email, O.DataUrodzenia, O.NumerTelefonu FROM Osoba O LEFT JOIN OsobaOdpowiedzialna OO ON O.IdOsoba = OO.IdOsoba LEFT JOIN Zawodnik Z ON O.IdOsoba = Z.IdOsoba WHERE Z.IdOsoba IS NULL AND O.IdOsoba!=@id";

					using (SqlCommand command = new SqlCommand(getPossibleHelpers, connection))
					{
						//pobieramy jsona z sesji
						string json = HttpContext.Session.GetString("LoggedUser");
						//deserializujemy jsona do obiektu o nazwie loggedUser, ktory bedzie zawiera³ informacje o aktualnie zalogowanym u¿ytkowniku
						Osoba loggedUser = JsonSerializer.Deserialize<Osoba>(json);

						command.Parameters.AddWithValue("@id", loggedUser.IdOsoba);

						using (SqlDataReader reader = command.ExecuteReader())
						{
							MozliwiPomocnicy = new List<Osoba>();
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
								MozliwiPomocnicy.Add(new Osoba(idOsoba, imie, nazwisko, email, dataUrodzenia, numerTelefonu));
							}
						}
					}

					if (MozliwiPomocnicy != null && MozliwiPomocnicy.Count > 0)
					{
						//Tworzenie pierwszego pustego rekordu
						var possibleHelpersList = new List<object>
						{
							new { Id=-9, Name="" },
							new { Id="", Name="Brak pomocnika" }
						};

						//Dodanie aktualnych osób w bazie do listy
						possibleHelpersList.AddRange(MozliwiPomocnicy.Select(p => new
						{
							Id = p.IdOsoba,
							Name = $"{p.Imie} {p.Nazwisko}, {p.DataUrodzenia:dd.MM.yyyy}, {p.Email}, {p.NumerTelefonu}"
						}));

						// Create the SelectList
						ViewData["possibleHelpersList"] = new SelectList(
							possibleHelpersList,
							"Id",
							"Name"
						);
					}
					else
					{
						ViewData["possibleHelpersList"] = new SelectList(new List<object>
						{
							new { Id=-9, Name="" },
							new { Id="", Name="Brak pomocnika" }
						}, 
						"Id", 
						"Name");
					}

					//pobranie danych osoby pe³ni¹cej dan¹ funkcje w celu wyœwietlenia na stronie
					string getHelperForFunction = "SELECT Osoba.Imie, Osoba.Nazwisko FROM Osoba INNER JOIN OsobaOdpowiedzialna ON Osoba.IdOsoba=OsobaOdpowiedzialna.IdOsoba WHERE OsobaOdpowiedzialna.IdWydarzenieSportowe = @idWydarzenia AND IdFunkcja = @idFunkcji";

					using(SqlCommand command = new SqlCommand(getHelperForFunction, connection))
					{
						command.Parameters.AddWithValue("@idWydarzenia", id);
						command.Parameters.AddWithValue("idFunkcji", 1);
						using(SqlDataReader reader = command.ExecuteReader())
						{
							string? imie = string.Empty;
							string? nazwisko = string.Empty;
							while(reader.Read())
							{
								imie = reader.GetString(0);
								nazwisko = reader.GetString(1);
							}
							if(imie.IsNullOrEmpty() || nazwisko.IsNullOrEmpty())
							{
								PomocnikDoObiektowSportowych = null;
							}
							else
							{
								PomocnikDoObiektowSportowych = new AktualnyPomocnik(imie, nazwisko);
							}
						}
					}

					using (SqlCommand command = new SqlCommand(getHelperForFunction, connection))
					{
						command.Parameters.AddWithValue("@idWydarzenia", id);
						command.Parameters.AddWithValue("idFunkcji", 2);
						using (SqlDataReader reader = command.ExecuteReader())
						{
							string? imie = string.Empty;
							string? nazwisko = string.Empty;
							while (reader.Read())
							{
								imie = reader.GetString(0);
								nazwisko = reader.GetString(1);
							}
							if (imie.IsNullOrEmpty() || nazwisko.IsNullOrEmpty())
							{
								PomocnikDoLokaliGastronomicznych = null;
							}
							else
							{
								PomocnikDoLokaliGastronomicznych = new AktualnyPomocnik(imie, nazwisko);
							}
						}
					}

					using (SqlCommand command = new SqlCommand(getHelperForFunction, connection))
					{
						command.Parameters.AddWithValue("@idWydarzenia", id);
						command.Parameters.AddWithValue("idFunkcji", 3);
						using (SqlDataReader reader = command.ExecuteReader())
						{
							string? imie = string.Empty;
							string? nazwisko = string.Empty;
							while (reader.Read())
							{
								imie = reader.GetString(0);
								nazwisko = reader.GetString(1);
							}
							if (imie.IsNullOrEmpty() || nazwisko.IsNullOrEmpty())
							{
								PomocnikDoSponsorow = null;
							}
							else
							{
								PomocnikDoSponsorow = new AktualnyPomocnik(imie, nazwisko);
							}
						}
					}

					using (SqlCommand command = new SqlCommand(getHelperForFunction, connection))
					{
						command.Parameters.AddWithValue("@idWydarzenia", id);
						command.Parameters.AddWithValue("idFunkcji", 4);
						using (SqlDataReader reader = command.ExecuteReader())
						{
							string? imie = string.Empty;
							string? nazwisko = string.Empty;
							while (reader.Read())
							{
								imie = reader.GetString(0);
								nazwisko = reader.GetString(1);
							}
							if (imie.IsNullOrEmpty() || nazwisko.IsNullOrEmpty())
							{
								PomocnikDoZawodnikow = null;
							}
							else
							{
								PomocnikDoZawodnikow = new AktualnyPomocnik(imie, nazwisko);
							}
						}
					}

					connection.Close();
				}
			}
            catch (Exception ex)
            {
				TempData["ErrorMessage"] = ex.Message;
				return;
			}
        }

		public void OnPost()
		{
			var eventId = HttpContext.Session.GetString("id");

			if (string.IsNullOrEmpty(eventId))
			{
				TempData["ErrorMessage"] = "Nieprawid³owe id wydarzenia!";
				return;
			}

			int IdWydarzenia = int.Parse(eventId);

			//tworzenie listy intów
			IList<int?> selectedHelpers = new List<int?>
			{
				WybranaOsobaDoObiektowSportowych,
				WybranaOsobaDoLokaliGastronomicznych,
				WybranaOsobaDoSposnorow,
				WybranaOsobaDoZawodnikow
			};

			int nullCounter = 0;

			for(int i=0; i < selectedHelpers.Count; i++)
			{
				if(selectedHelpers.ElementAt(i) == null | selectedHelpers.ElementAt(i) < 1)
				{
					nullCounter++;
				}
			}

			/*
			List<int> selectedHelpersWithoutNulls = selectedHelpers.Where(helper => helper.HasValue).Where(helper => helper.Value > 0).Select(helper => helper.Value).ToList();

			//sprawdzamy czy s¹ duplikaty
			if(selectedHelpersWithoutNulls.Count != selectedHelpersWithoutNulls.Distinct().Count())
			{
				TempData["ErrorMessage"] = "Ta sama osoba nie mo¿e zostaæ przypisana do tej samej funkcji!";
				//znów przywracamy dane ze strony tym razem z zachowanym wydarzeniem sportowym
				//uzywamy funkcji LoadEventData(int id)
				LoadEventData(IdWydarzenia);
				return;
			}
			*/

			string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

			try
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();

					string updateHelper = "UPDATE OsobaOdpowiedzialna SET IdOsoba=@idOsoby WHERE IdWydarzenieSportowe=@idWydarzenia AND IdFunkcja=@idFunkcji";

					//wykorzystanie kwerendy najpierw do przypisania funkcji znalezienia obiektów sportowych
					//a potem do reszty gdzie jedynym patrametrem nie zmienionym jest @idWydarzenia, bo przypisujemy
					//te wszystkie funkcje do jednego wydarzenia sportowego.
					using (SqlCommand command = new SqlCommand(updateHelper, connection))
					{
						if(WybranaOsobaDoObiektowSportowych == null)
						{
							command.Parameters.AddWithValue("@idOsoby", DBNull.Value);
							command.Parameters.AddWithValue("@idWydarzenia", IdWydarzenia);
							command.Parameters.AddWithValue("@idFunkcji", 1);
							command.ExecuteScalar();
						}
						else if(WybranaOsobaDoObiektowSportowych > 0)
						{
							command.Parameters.AddWithValue("@idOsoby", WybranaOsobaDoObiektowSportowych);
							command.Parameters.AddWithValue("@idWydarzenia", IdWydarzenia);
							command.Parameters.AddWithValue("@idFunkcji", 1);
							command.ExecuteScalar();
						}
						else
						{
							command.Cancel();
						}
					}

					//kolejne wykorzystanie kwerendy tym razem do funkcji znalezienia odpowiednich
					//obiektów gastronomicznych
					using (SqlCommand command = new SqlCommand(updateHelper, connection))
					{
						if (WybranaOsobaDoLokaliGastronomicznych == null)
						{
							command.Parameters.AddWithValue("@idOsoby", DBNull.Value);
							command.Parameters.AddWithValue("@idWydarzenia", IdWydarzenia);
							command.Parameters.AddWithValue("@idFunkcji", 2);
							command.ExecuteScalar();
						}
						else if (WybranaOsobaDoLokaliGastronomicznych > 0)
						{
							command.Parameters.AddWithValue("@idOsoby", WybranaOsobaDoLokaliGastronomicznych);
							command.Parameters.AddWithValue("@idWydarzenia", IdWydarzenia);
							command.Parameters.AddWithValue("@idFunkcji", 2);
							command.ExecuteScalar();
						}
						else
						{
							command.Cancel();
						}
					}

					//kolejne wykorzystanie kwerendy tym razem do funkcji znalezienia odpowiednich
					//sponsorów
					using (SqlCommand command = new SqlCommand(updateHelper, connection))
					{
						if (WybranaOsobaDoSposnorow == null)
						{
							command.Parameters.AddWithValue("@idOsoby", DBNull.Value);
							command.Parameters.AddWithValue("@idWydarzenia", IdWydarzenia);
							command.Parameters.AddWithValue("@idFunkcji", 3);
							command.ExecuteScalar();
						}
						else if (WybranaOsobaDoSposnorow > 0)
						{
							command.Parameters.AddWithValue("@idOsoby", WybranaOsobaDoSposnorow);
							command.Parameters.AddWithValue("@idWydarzenia", IdWydarzenia);
							command.Parameters.AddWithValue("@idFunkcji", 3);
							command.ExecuteScalar();
						}
						else
						{
							command.Cancel();
						}
					}

					//kolejne wykorzystanie kwerendy tym razem do funkcji organizacji zawodników
					using (SqlCommand command = new SqlCommand(updateHelper, connection))
					{
						if (WybranaOsobaDoZawodnikow == null)
						{
							command.Parameters.AddWithValue("@idOsoby", DBNull.Value);
							command.Parameters.AddWithValue("@idWydarzenia", IdWydarzenia);
							command.Parameters.AddWithValue("@idFunkcji", 4);
							command.ExecuteScalar();
						}
						else if (WybranaOsobaDoZawodnikow > 0)
						{
							command.Parameters.AddWithValue("@idOsoby", WybranaOsobaDoZawodnikow);
							command.Parameters.AddWithValue("@idWydarzenia", IdWydarzenia);
							command.Parameters.AddWithValue("@idFunkcji", 4);
							command.ExecuteScalar();
						}
						else
						{
							command.Cancel();
						}
					}

					connection.Close();

					TempData["SuccessMessage"] = "Pomyœlnie przypisano osoby do poszczególnych funkcji!";
					LoadEventData(IdWydarzenia);
					return;
				}
			}
			catch(Exception ex)
			{
				TempData["ErrorMessage"] = ex.Message;
				LoadEventData(IdWydarzenia);
				return;
			}
		}
		private void LoadEventData(int id)
		{
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

					//tworzenie listy mo¿liwych pomocników
					string getPossibleHelpers = "SELECT DISTINCT O.IdOsoba, O.Imie, O.Nazwisko, O.Email, O.DataUrodzenia, O.NumerTelefonu FROM Osoba O LEFT JOIN OsobaOdpowiedzialna OO ON O.IdOsoba = OO.IdOsoba LEFT JOIN Zawodnik Z ON O.IdOsoba = Z.IdOsoba WHERE Z.IdOsoba IS NULL AND O.IdOsoba!=@id";

					using (SqlCommand command = new SqlCommand(getPossibleHelpers, connection))
					{
						//pobieramy jsona z sesji
						string json = HttpContext.Session.GetString("LoggedUser");
						//deserializujemy jsona do obiektu o nazwie loggedUser, ktory bedzie zawiera³ informacje o aktualnie zalogowanym u¿ytkowniku
						Osoba loggedUser = JsonSerializer.Deserialize<Osoba>(json);

						command.Parameters.AddWithValue("@id", loggedUser.IdOsoba);

						using (SqlDataReader reader = command.ExecuteReader())
						{
							MozliwiPomocnicy = new List<Osoba>();
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
								MozliwiPomocnicy.Add(new Osoba(idOsoba, imie, nazwisko, email, dataUrodzenia, numerTelefonu));
							}
						}
					}

					if (MozliwiPomocnicy != null && MozliwiPomocnicy.Count > 0)
					{
						//Tworzenie pierwszego pustego rekordu
						var possibleHelpersList = new List<object>
						{
							new { Id=-9, Name="" },
							new { Id=0, Name="Brak pomocnika" }
						};

						//Dodanie aktualnych osób w bazie do listy
						possibleHelpersList.AddRange(MozliwiPomocnicy.Select(p => new
						{
							Id = p.IdOsoba,
							Name = $"{p.Imie} {p.Nazwisko}, {p.DataUrodzenia:dd.MM.yyyy}, {p.Email}, {p.NumerTelefonu}"
						}));

						// Create the SelectList
						ViewData["possibleHelpersList"] = new SelectList(
							possibleHelpersList,
							"Id",
							"Name"
						);
					}
					else
					{
						ViewData["possibleHelpersList"] = new SelectList(new List<object>
						{
							new { Id=-9, Name="" },
							new { Id=0, Name="Brak pomocnika" }
						},
						"Id",
						"Name");
					}

					//pobranie danych osoby pe³ni¹cej dan¹ funkcje w celu wyœwietlenia na stronie
					string getHelperForFunction = "SELECT Osoba.Imie, Osoba.Nazwisko FROM Osoba INNER JOIN OsobaOdpowiedzialna ON Osoba.IdOsoba=OsobaOdpowiedzialna.IdOsoba WHERE OsobaOdpowiedzialna.IdWydarzenieSportowe = @idWydarzenia AND IdFunkcja = @idFunkcji";

					using (SqlCommand command = new SqlCommand(getHelperForFunction, connection))
					{
						command.Parameters.AddWithValue("@idWydarzenia", id);
						command.Parameters.AddWithValue("idFunkcji", 1);
						using (SqlDataReader reader = command.ExecuteReader())
						{
							string? imie = string.Empty;
							string? nazwisko = string.Empty;
							while (reader.Read())
							{
								imie = reader.GetString(0);
								nazwisko = reader.GetString(1);
							}
							if (imie.IsNullOrEmpty() || nazwisko.IsNullOrEmpty())
							{
								PomocnikDoObiektowSportowych = null;
							}
							else
							{
								PomocnikDoObiektowSportowych = new AktualnyPomocnik(imie, nazwisko);
							}
						}
					}

					using (SqlCommand command = new SqlCommand(getHelperForFunction, connection))
					{
						command.Parameters.AddWithValue("@idWydarzenia", id);
						command.Parameters.AddWithValue("idFunkcji", 2);
						using (SqlDataReader reader = command.ExecuteReader())
						{
							string? imie = string.Empty;
							string? nazwisko = string.Empty;
							while (reader.Read())
							{
								imie = reader.GetString(0);
								nazwisko = reader.GetString(1);
							}
							if (imie.IsNullOrEmpty() || nazwisko.IsNullOrEmpty())
							{
								PomocnikDoLokaliGastronomicznych = null;
							}
							else
							{
								PomocnikDoLokaliGastronomicznych = new AktualnyPomocnik(imie, nazwisko);
							}
						}
					}

					using (SqlCommand command = new SqlCommand(getHelperForFunction, connection))
					{
						command.Parameters.AddWithValue("@idWydarzenia", id);
						command.Parameters.AddWithValue("idFunkcji", 3);
						using (SqlDataReader reader = command.ExecuteReader())
						{
							string? imie = string.Empty;
							string? nazwisko = string.Empty;
							while (reader.Read())
							{
								imie = reader.GetString(0);
								nazwisko = reader.GetString(1);
							}
							if (imie.IsNullOrEmpty() || nazwisko.IsNullOrEmpty())
							{
								PomocnikDoSponsorow = null;
							}
							else
							{
								PomocnikDoSponsorow = new AktualnyPomocnik(imie, nazwisko);
							}
						}
					}

					using (SqlCommand command = new SqlCommand(getHelperForFunction, connection))
					{
						command.Parameters.AddWithValue("@idWydarzenia", id);
						command.Parameters.AddWithValue("idFunkcji", 4);
						using (SqlDataReader reader = command.ExecuteReader())
						{
							string? imie = string.Empty;
							string? nazwisko = string.Empty;
							while (reader.Read())
							{
								imie = reader.GetString(0);
								nazwisko = reader.GetString(1);
							}
							if (imie.IsNullOrEmpty() || nazwisko.IsNullOrEmpty())
							{
								PomocnikDoZawodnikow = null;
							}
							else
							{
								PomocnikDoZawodnikow = new AktualnyPomocnik(imie, nazwisko);
							}
						}
					}

					connection.Close();
				}
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = ex.Message;
				return;
			}
		}
    }
}
