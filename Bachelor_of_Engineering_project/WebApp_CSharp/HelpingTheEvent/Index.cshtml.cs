using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.Inne;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.AddHelpers;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.HelpingTheEvent
{
    public class IndexModel : PageModel
    {
		public WydarzenieSportowe? WydarzenieSportowe { get; set; }
		public IList<DyscyplinaOrazData>? DyscyplinyOrazDaty { get; set; }
		public Osoba? Organizator { get; set; }
		public IList<Funkcja>? Funkcje { get; set; }
		public IList<Zawody> Zawody { get; set; }
		public IList<ObiektSportowy>? ObiektySportowe { get; set; }
		public IList<LokalGastronomiczny>? LokaleGastronomiczne { get; set; }
		public IList<Sponsor>? Sponsorzy { get; set; }
		public int IloscDyscyplin { get; set; }
		public IList<AktualnyObiektSportowy?>? AktualneObiektySportowe { get; set; }
		public IList<AktualnyLokalGastronomiczny?>? AktualneLokaleGastronomiczne { get; set; }
		public IList<AktualnySponsor?>? AktualniSponsorzy { get; set; }
		[BindProperty]
		public IList<int?>? WybraneObiektySportowe { get; set; } = new List<int?>();
		[BindProperty]
		public IList<int?>? WybraneLokaleGastronomiczne { get; set; } = new List<int?>();
		[BindProperty]
		public IList<int?>? WybraniSponsorzy { get; set; } = new List<int?>();

		public void OnGet(int id)
		{
			HttpContext.Session.Remove("idZawodnik");
            HttpContext.Session.Remove("idWydarzenieSportowe");
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

					string getRoleNameAndDescription = "SELECT Funkcja.IdFunkcja, Funkcja.Nazwa, Funkcja.Opis FROM Funkcja INNER JOIN OsobaOdpowiedzialna ON Funkcja.IdFunkcja = OsobaOdpowiedzialna.IdFunkcja WHERE OsobaOdpowiedzialna.IdWydarzenieSportowe = @idWydarzenia AND OsobaOdpowiedzialna.IdOsoba = @idOsoby";

					using (SqlCommand command = new SqlCommand(getRoleNameAndDescription, connection))
					{
						//pobieramy jsona z sesji
						string json = HttpContext.Session.GetString("LoggedUser");
						//deserializujemy jsona do obiektu o nazwie loggedUser, ktory bedzie zawiera³ informacje o aktualnie zalogowanym u¿ytkowniku
						Osoba loggedUser = JsonSerializer.Deserialize<Osoba>(json);

						command.Parameters.AddWithValue("@idWydarzenia", id);
						command.Parameters.AddWithValue("@idOsoby", loggedUser.IdOsoba);

						using (SqlDataReader reader = command.ExecuteReader())
						{
							Funkcje = new List<Funkcja>();
							int idFunkcja;
							string nazwa;
							string opis;
							while (reader.Read())
							{
								idFunkcja = reader.GetInt32(0);
								nazwa = reader.GetString(1);
								opis = reader.GetString(2);
								Funkcje.Add(new Funkcja(idFunkcja, nazwa, opis));
							}
						}
					}

					string getIloscDyscyplin = "SELECT COUNT(*) FROM Zawody INNER JOIN WydarzenieSportowe ON Zawody.IdWydarzenieSportowe = WydarzenieSportowe.IdWydarzenieSportowe WHERE Zawody.IdWydarzenieSportowe = @id";

					using (SqlCommand command = new SqlCommand(getIloscDyscyplin, connection))
					{
						command.Parameters.AddWithValue("@id", id);
						IloscDyscyplin = (int)command.ExecuteScalar();
					}

					string getPossibleSportFacilities = "SELECT IdObiektSportowy, Nazwa, Miejscowosc, Ulica, NumerBudynku, KodPocztowy FROM ObiektSportowy";

					using (SqlCommand command = new SqlCommand(getPossibleSportFacilities, connection))
					{
						using (SqlDataReader reader = command.ExecuteReader())
						{
							ObiektySportowe = new List<ObiektSportowy>();
							int idObiektSportowy;
							string nazwa;
							string miejscowosc;
							string ulica;
							string numerBudynku;
							string kodPocztowy;
							while (reader.Read())
							{
								idObiektSportowy = reader.GetInt32(0);
								nazwa = reader.GetString(1);
								miejscowosc = reader.GetString(2);
								ulica = reader.GetString(3);
								numerBudynku = reader.GetString(4);
								kodPocztowy = reader.GetString(5);
								ObiektySportowe.Add(new ObiektSportowy(idObiektSportowy, nazwa, miejscowosc, ulica, numerBudynku, kodPocztowy));
							}
						}
					}

					if (ObiektySportowe != null && ObiektySportowe.Count > 0)
					{
						//Tworzenie pierwszego pustego rekordu
						var possibleSportFacilityList = new List<object>
						{
							new { Id=-9, Name="" },
							new { Id="", Name="Brak obiektu sportowego" }
						};

						//Dodanie aktualnych osób w bazie do listy
						possibleSportFacilityList.AddRange(ObiektySportowe.Select(os => new
						{
							Id = os.IdObiektSportowy,
							Name = $"{os.Nazwa}, {os.Miejscowosc}, {os.Ulica}, {os.NumerBudynku}, {os.KodPocztowy}"
						}));

						// Create the SelectList
						ViewData["possibleSportFacilityList"] = new SelectList(
							possibleSportFacilityList,
							"Id",
							"Name"
						);
					}
					else
					{
						ViewData["possibleSportFacilityList"] = new SelectList(new List<object>
						{
							new { Id=-9, Name="" },
							new { Id="", Name="Brak obiektu sportowego" }
						},
						"Id",
						"Name");
					}

					string getPossibleTreatFacilities = "SELECT IdLokalGastronomiczny, Nazwa, Miejscowosc, Ulica, NumerBudynku, KodPocztowy FROM LokalGastronomiczny";

					using (SqlCommand command = new SqlCommand(getPossibleTreatFacilities, connection))
					{
						using (SqlDataReader reader = command.ExecuteReader())
						{
							LokaleGastronomiczne = new List<LokalGastronomiczny>();
							int idLokalGastronomiczny;
							string nazwa;
							string miejscowosc;
							string ulica;
							string numerBudynku;
							string kodPocztowy;
							while (reader.Read())
							{
								idLokalGastronomiczny = reader.GetInt32(0);
								nazwa = reader.GetString(1);
								miejscowosc = reader.GetString(2);
								ulica = reader.GetString(3);
								numerBudynku = reader.GetString(4);
								kodPocztowy = reader.GetString(5);
								LokaleGastronomiczne.Add(new LokalGastronomiczny(idLokalGastronomiczny, nazwa, miejscowosc, ulica, numerBudynku, kodPocztowy));
							}
						}
					}

					if (LokaleGastronomiczne != null && LokaleGastronomiczne.Count > 0)
					{
						//Tworzenie pierwszego pustego rekordu
						var possibleTreatFacilityList = new List<object>
						{
							new { Id=-9, Name="" },
							new { Id="", Name="Brak lokalu gastronomicznego" }
						};

						//Dodanie aktualnych osób w bazie do listy
						possibleTreatFacilityList.AddRange(LokaleGastronomiczne.Select(lg => new
						{
							Id = lg.IdLokalGastronomiczny,
							Name = $"{lg.Nazwa}, {lg.Miejscowosc}, {lg.Ulica}, {lg.NumerBudynku}, {lg.KodPocztowy}"
						}));

						// Create the SelectList
						ViewData["possibleTreatFacilityList"] = new SelectList(
							possibleTreatFacilityList,
							"Id",
							"Name"
						);
					}
					else
					{
						ViewData["possibleTreatFacilityList"] = new SelectList(new List<object>
						{
							new { Id=-9, Name="" },
							new { Id="", Name="Brak lokalu gastronomicznego" }
						},
						"Id",
						"Name");
					}

					string getPossibleSponsors = "SELECT IdSponsor, Nazwa, Miejscowosc, Ulica, NumerBudynku, KodPocztowy FROM Sponsor";

					using (SqlCommand command = new SqlCommand(getPossibleSponsors, connection))
					{
						using (SqlDataReader reader = command.ExecuteReader())
						{
							Sponsorzy = new List<Sponsor>();
							int idSponsor;
							string nazwa;
							string miejscowosc;
							string ulica;
							string numerBudynku;
							string kodPocztowy;
							while (reader.Read())
							{
								idSponsor = reader.GetInt32(0);
								nazwa = reader.GetString(1);
								miejscowosc = reader.GetString(2);
								ulica = reader.GetString(3);
								numerBudynku = reader.GetString(4);
								kodPocztowy = reader.GetString(5);
								Sponsorzy.Add(new Sponsor(idSponsor, nazwa, miejscowosc, ulica, numerBudynku, kodPocztowy));
							}
						}
					}

					if (Sponsorzy != null && Sponsorzy.Count > 0)
					{
						//Tworzenie pierwszego pustego rekordu
						var possibleSponsorList = new List<object>
						{
							new { Id=-9, Name="" },
							new { Id="", Name="Brak sponsora" }
						};

						//Dodanie aktualnych osób w bazie do listy
						possibleSponsorList.AddRange(Sponsorzy.Select(s => new
						{
							Id = s.IdSponsor,
							Name = $"{s.Nazwa}, {s.Miejscowosc}, {s.Ulica}, {s.NumerBudynku}, {s.KodPocztowy}"
						}));

						// Create the SelectList
						ViewData["possibleSponsorList"] = new SelectList(
							possibleSponsorList,
							"Id",
							"Name"
						);
					}
					else
					{
						ViewData["possibleSponsorList"] = new SelectList(new List<object>
						{
							new { Id=-9, Name="" },
							new { Id="", Name="Brak sponsora" }
						},
						"Id",
						"Name");
					}

					//pobieramy jsona z sesji
					string json2 = HttpContext.Session.GetString("LoggedUser");
					//deserializujemy jsona do obiektu o nazwie loggedUser, ktory bedzie zawiera³ informacje o aktualnie zalogowanym u¿ytkowniku
					Osoba loggedUser2 = JsonSerializer.Deserialize<Osoba>(json2);

					string getFunctionsThatUserIsDoing = "SELECT IdFunkcja FROM OsobaOdpowiedzialna WHERE OsobaOdpowiedzialna.IdWydarzenieSportowe = @idWydarzenia AND OsobaOdpowiedzialna.IdOsoba = @idOsoby";

					List<int>? idFunkcji = new List<int>();

					using (SqlCommand command = new SqlCommand(getFunctionsThatUserIsDoing, connection))
					{
						command.Parameters.AddWithValue("@idWydarzenia", id);
						command.Parameters.AddWithValue("@idOsoby", loggedUser2.IdOsoba);
						using (SqlDataReader reader = command.ExecuteReader())
						{
							int idFunkcja;
							while (reader.Read())
							{
								idFunkcja = reader.GetInt32(0);
								idFunkcji.Add(idFunkcja);
							}
						}
					}

					foreach(var idFunkcja in idFunkcji)
					{ 
						switch (idFunkcja)
						{
							case 1:
								string getCurrentlySetSportFacilityForDiscipline = "SELECT ObiektSportowy.Nazwa FROM ObiektSportowy INNER JOIN Zawody ON ObiektSportowy.IdObiektSportowy = Zawody.IdObiektSportowy INNER JOIN WydarzenieSportowe ON Zawody.IdWydarzenieSportowe = WydarzenieSportowe.IdWydarzenieSportowe WHERE WydarzenieSportowe.IdWydarzenieSportowe = @idWydarzenia AND Zawody.IdDyscyplina = @IdDyscyplina";

								AktualneObiektySportowe = new List<AktualnyObiektSportowy?>();

								for (int i = 0; i < DyscyplinyOrazDaty.Count; i++)
								{
									using (SqlCommand command = new SqlCommand(getCurrentlySetSportFacilityForDiscipline, connection))
									{
										command.Parameters.AddWithValue("@idWydarzenia", id);
										command.Parameters.AddWithValue("@IdDyscyplina", DyscyplinyOrazDaty.ElementAt(i).IdDyscyplina);
										string? facilityName = (string)command.ExecuteScalar();
										if (facilityName == null)
										{
											AktualneObiektySportowe.Add(null);
										}
										else
										{
											AktualneObiektySportowe.Add(new AktualnyObiektSportowy(facilityName));
										}
									}
								}
								break;

							case 2:
								string getCurrentlySetTreatFacilityForDiscpiline = "SELECT LokalGastronomiczny.Nazwa FROM LokalGastronomiczny INNER JOIN Poczestunek ON LokalGastronomiczny.IdLokalGastronomiczny = Poczestunek.IdLokalGastronomiczny INNER JOIN Zawody ON Poczestunek.IdZawody = Zawody.IdZawody WHERE Zawody.IdWydarzenieSportowe = @idWydarzenia AND Zawody.IdDyscyplina = @IdDyscyplina";

								AktualneLokaleGastronomiczne = new List<AktualnyLokalGastronomiczny?>();

								for (int i = 0; i < DyscyplinyOrazDaty.Count; i++)
								{
									using (SqlCommand command = new SqlCommand(getCurrentlySetTreatFacilityForDiscpiline, connection))
									{
										command.Parameters.AddWithValue("idWydarzenia", id);
										command.Parameters.AddWithValue("@IdDyscyplina", DyscyplinyOrazDaty.ElementAt(i).IdDyscyplina);
										string? facilityName = (string)command.ExecuteScalar();
										if (facilityName == null)
										{
											AktualneLokaleGastronomiczne.Add(null);
										}
										else
										{
											AktualneLokaleGastronomiczne.Add(new AktualnyLokalGastronomiczny(facilityName));
										}
									}
								}
								break;

							case 3:
								string getCurrentlySetSponsorForDiscpiline = "SELECT Sponsor.Nazwa FROM Sponsor INNER JOIN SponsorZawodow ON Sponsor.IdSponsor = SponsorZawodow.IdSponsor INNER JOIN Zawody ON SponsorZawodow.IdZawody = Zawody.IdZawody WHERE Zawody.IdWydarzenieSportowe = @idWydarzenia AND Zawody.IdDyscyplina = @IdDyscyplina";

								AktualniSponsorzy = new List<AktualnySponsor?>();

								for (int i = 0; i < DyscyplinyOrazDaty.Count; i++)
								{
									using (SqlCommand command = new SqlCommand(getCurrentlySetSponsorForDiscpiline, connection))
									{
										command.Parameters.AddWithValue("idWydarzenia", id);
										command.Parameters.AddWithValue("@IdDyscyplina", DyscyplinyOrazDaty.ElementAt(i).IdDyscyplina);
										string? sponsorName = (string)command.ExecuteScalar();
										if (sponsorName == null)
										{
											AktualniSponsorzy.Add(null);
										}
										else
										{
											AktualniSponsorzy.Add(new AktualnySponsor(sponsorName));
										}
									}
								}
								break;
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

		public void OnPost(List<int> WybraneZawody)
		{
			var eventId = HttpContext.Session.GetString("eventId");

			if (string.IsNullOrEmpty(eventId))
			{
				TempData["ErrorMessage"] = "Nieprawid³owe id wydarzenia!";
				return;
			}

			int IdWydarzenia = int.Parse(eventId);

			string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

			try
			{
				using(SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();

					string getDyscyplinyOrazDaty = "SELECT Dyscyplina.IdDyscyplina, Dyscyplina.Nazwa, Zawody.Data FROM Dyscyplina INNER JOIN Zawody ON Dyscyplina.IdDyscyplina = Zawody.IdDyscyplina INNER JOIN WydarzenieSportowe ON Zawody.IdWydarzenieSportowe = WydarzenieSportowe.IdWydarzenieSportowe WHERE WydarzenieSportowe.IdWydarzenieSportowe = @id";

					using (SqlCommand command = new SqlCommand(getDyscyplinyOrazDaty, connection))
					{
						command.Parameters.AddWithValue("@id", IdWydarzenia);

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
						command.Parameters.AddWithValue("@id", IdWydarzenia);

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

					//gdy zalogowany u¿ytkownik jest odpowiedzialny za organizacje obiektów sportowych
					if (!WybraneObiektySportowe.IsNullOrEmpty())
					{
						string updateSportFacility = "UPDATE Zawody SET IdObiektSportowy=@idObiektuSportowego WHERE IdDyscyplina=@IdDyscyplina AND IdWydarzenieSportowe=@idWydarzeniaSportowego";
						
						for(int i = 0; i < DyscyplinyOrazDaty.Count; i++)
						{
							using(SqlCommand command = new SqlCommand(updateSportFacility, connection))
							{
								if (WybraneObiektySportowe.ElementAt(i) == null)
								{
									command.Parameters.AddWithValue("@idObiektuSportowego", DBNull.Value);
									command.Parameters.AddWithValue("@IdDyscyplina", DyscyplinyOrazDaty.ElementAt(i).IdDyscyplina);
									command.Parameters.AddWithValue("@idWydarzeniaSportowego", IdWydarzenia);
									command.ExecuteScalar();
								}
								else if(WybraneObiektySportowe.ElementAt(i) > 0)
								{
									command.Parameters.AddWithValue("@idObiektuSportowego", WybraneObiektySportowe.ElementAt(i));
									command.Parameters.AddWithValue("@IdDyscyplina", DyscyplinyOrazDaty.ElementAt(i).IdDyscyplina);
									command.Parameters.AddWithValue("@idWydarzeniaSportowego", IdWydarzenia);
									command.ExecuteScalar();
								}
								else
								{
									command.Cancel();
								}
							}
						}
						TempData["SuccessMessage"] = "Pomyœlnie przypisano obiekty sportowe do zawodów!";
						LoadEventData(IdWydarzenia);
						return;
					}

					//gdy zalogowany u¿ytkownik jest odpowiedzialny za organizacje obiektów gastronomicznych
					if (!WybraneLokaleGastronomiczne.IsNullOrEmpty())
					{
						string updateTreatFacility = "UPDATE Poczestunek SET IdLokalGastronomiczny=@idLokaluGastronomicznego WHERE IdZawody=@idZawodow";

						for(int i = 0; i < DyscyplinyOrazDaty.Count; i++)
						{
							using(SqlCommand command = new SqlCommand(updateTreatFacility, connection))
							{
								if(WybraneLokaleGastronomiczne.ElementAt(i) == null)
								{
									command.Parameters.AddWithValue("@idLokaluGastronomicznego", DBNull.Value);
									command.Parameters.AddWithValue("@idZawodow", Zawody.ElementAt(i).IdZawody);
									command.ExecuteScalar();
								}
								else if(WybraneLokaleGastronomiczne.ElementAt(i) > 0)
								{
									command.Parameters.AddWithValue("@idLokaluGastronomicznego", WybraneLokaleGastronomiczne.ElementAt(i));
									command.Parameters.AddWithValue("@idZawodow", Zawody.ElementAt(i).IdZawody);
									command.ExecuteScalar();
								}
								else
								{
									command.Cancel();
								}
							}
						}
						TempData["SuccessMessage"] = "Pomyœlnie przypisano lokale gastronomiczne do zawodów!";
						LoadEventData(IdWydarzenia);
						return;
					}

					//gdy zalogowany u¿ytkownik jest odpowiedzialny za organizacje sponsorów
					if (!WybraniSponsorzy.IsNullOrEmpty())
					{
						string updateSponsor = "UPDATE SponsorZawodow SET IdSponsor=@idSponsora WHERE IdZawody=@idZawodow";

						for (int i = 0; i < DyscyplinyOrazDaty.Count; i++)
						{
							using (SqlCommand command = new SqlCommand(updateSponsor, connection))
							{
								if (WybraniSponsorzy.ElementAt(i) == null)
								{
									command.Parameters.AddWithValue("@idSponsora", DBNull.Value);
									command.Parameters.AddWithValue("@idZawodow", Zawody.ElementAt(i).IdZawody);
									command.ExecuteScalar();
								}
								else if (WybraniSponsorzy.ElementAt(i) > 0)
								{
									command.Parameters.AddWithValue("@idSponsora", WybraniSponsorzy.ElementAt(i));
									command.Parameters.AddWithValue("@idZawodow", Zawody.ElementAt(i).IdZawody);
									command.ExecuteScalar();
								}
								else
								{
									command.Cancel();
								}
							}
						}
						TempData["SuccessMessage"] = "Pomyœlnie przypisano sponsorów do zawodów!";
						LoadEventData(IdWydarzenia);
						return;
					}

					//gdy zalogowany u¿ytkownik jest odpowiedzialny za organizacje zawodników
					if (WybraneObiektySportowe.IsNullOrEmpty() && WybraneLokaleGastronomiczne.IsNullOrEmpty() && WybraniSponsorzy.IsNullOrEmpty())
					{
						if(!WybraneZawody.IsNullOrEmpty())
						{
							string imie = Request.Form["imie"];
							string nazwisko = Request.Form["nazwisko"];
							string email = Request.Form["email"];
							if (DateTime.TryParse(Request.Form["dataUrodzenia"], out DateTime dataUrodzenia)) { }
							else
							{
								TempData["ErrorMessage"] = "Podano nieprawid³owy format daty!";
								LoadEventData(IdWydarzenia);
								return;
							}
							string numerTelefonu = Request.Form["numerTelefonu"];

							if (imie.IsNullOrEmpty() || nazwisko.IsNullOrEmpty() || email.IsNullOrEmpty() || numerTelefonu.IsNullOrEmpty())
							{
								TempData["ErrorMessage"] = "Wszystkie pola musz¹ zostaæ wype³nione!";
								LoadEventData(IdWydarzenia);
								return;
							}

							PrzypisanyZawodnik playerToAdd = new PrzypisanyZawodnik(imie, nazwisko, email, dataUrodzenia, numerTelefonu, WybraneZawody);

							string addPlayerAsPerson = "INSERT INTO Osoba(Imie, Nazwisko, Email, DataUrodzenia, NumerTelefonu) VALUES(@imie, @nazwisko, @email, @dataUrodzenia, @numerTelefonu); SELECT SCOPE_IDENTITY()";

							using (SqlCommand command = new SqlCommand(addPlayerAsPerson, connection))
							{
								command.Parameters.AddWithValue("@imie", playerToAdd.Imie);
								command.Parameters.AddWithValue("@nazwisko", playerToAdd.Nazwisko);
								command.Parameters.AddWithValue("@email", playerToAdd.Email);
								command.Parameters.AddWithValue("@dataUrodzenia", playerToAdd.DataUrodzenia);
								command.Parameters.AddWithValue("@numerTelefonu", playerToAdd.NumerTelefonu);

								object wynik = command.ExecuteScalar();

								if (wynik != null)
								{
									playerToAdd.IdOsoba = Convert.ToInt32(wynik);
								}
								else
								{
									TempData["ErrorMessage"] = "Nie uda³o siê uzyskaæ wartoœci pola IdOsoba z tabeli Osoba";
									LoadEventData(IdWydarzenia);
									return;
								}
							}

							//teraz dodajemy rekordy do tabeli Zawodnik w zale¿noœci na ile zawodów siê zapisa³.
							string addPlayer = "INSERT INTO Zawodnik(IdZawody, IdOsoba) VALUES(@idZawodow, @idOsoby)";
							
							for(int i=0; i < WybraneZawody.Count; i++)
							{
								using(SqlCommand command = new SqlCommand(addPlayer, connection))
								{
									command.Parameters.AddWithValue("@idZawodow", WybraneZawody.ElementAt(i));
									command.Parameters.AddWithValue("@idOsoby", playerToAdd.IdOsoba);

									command.ExecuteNonQuery();
								}
							}

							TempData["SuccessMessage"] = "Pomyœlnie dodano zawodnika!";
							LoadEventData(IdWydarzenia);
							return;

						}
						else
						{
							TempData["ErrorMessage"] = "Nale¿y zaznaczyæ przynajmniej jedne zawody na które zawodnik chce siê zapisaæ";
							LoadEventData(IdWydarzenia);
							return;
						}
					}

					connection.Close();
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
            HttpContext.Session.Remove("idZawodnik");
            HttpContext.Session.Remove("idWydarzenieSportowe");

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

					string getRoleNameAndDescription = "SELECT Funkcja.IdFunkcja, Funkcja.Nazwa, Funkcja.Opis FROM Funkcja INNER JOIN OsobaOdpowiedzialna ON Funkcja.IdFunkcja = OsobaOdpowiedzialna.IdFunkcja WHERE OsobaOdpowiedzialna.IdWydarzenieSportowe = @idWydarzenia AND OsobaOdpowiedzialna.IdOsoba = @idOsoby";

					using (SqlCommand command = new SqlCommand(getRoleNameAndDescription, connection))
					{
						//pobieramy jsona z sesji
						string json = HttpContext.Session.GetString("LoggedUser");
						//deserializujemy jsona do obiektu o nazwie loggedUser, ktory bedzie zawiera³ informacje o aktualnie zalogowanym u¿ytkowniku
						Osoba loggedUser = JsonSerializer.Deserialize<Osoba>(json);

						command.Parameters.AddWithValue("@idWydarzenia", id);
						command.Parameters.AddWithValue("@idOsoby", loggedUser.IdOsoba);

						using (SqlDataReader reader = command.ExecuteReader())
						{
							Funkcje = new List<Funkcja>();
							int idFunkcja;
							string nazwa;
							string opis;
							while (reader.Read())
							{
								idFunkcja = reader.GetInt32(0);
								nazwa = reader.GetString(1);
								opis = reader.GetString(2);
								Funkcje.Add(new Funkcja(idFunkcja, nazwa, opis));
							}
						}
					}

					string getIloscDyscyplin = "SELECT COUNT(*) FROM Zawody INNER JOIN WydarzenieSportowe ON Zawody.IdWydarzenieSportowe = WydarzenieSportowe.IdWydarzenieSportowe WHERE Zawody.IdWydarzenieSportowe = @id";

					using (SqlCommand command = new SqlCommand(getIloscDyscyplin, connection))
					{
						command.Parameters.AddWithValue("@id", id);
						IloscDyscyplin = (int)command.ExecuteScalar();
					}

					string getPossibleSportFacilities = "SELECT IdObiektSportowy, Nazwa, Miejscowosc, Ulica, NumerBudynku, KodPocztowy FROM ObiektSportowy";

					using (SqlCommand command = new SqlCommand(getPossibleSportFacilities, connection))
					{
						using (SqlDataReader reader = command.ExecuteReader())
						{
							ObiektySportowe = new List<ObiektSportowy>();
							int idObiektSportowy;
							string nazwa;
							string miejscowosc;
							string ulica;
							string numerBudynku;
							string kodPocztowy;
							while (reader.Read())
							{
								idObiektSportowy = reader.GetInt32(0);
								nazwa = reader.GetString(1);
								miejscowosc = reader.GetString(2);
								ulica = reader.GetString(3);
								numerBudynku = reader.GetString(4);
								kodPocztowy = reader.GetString(5);
								ObiektySportowe.Add(new ObiektSportowy(idObiektSportowy, nazwa, miejscowosc, ulica, numerBudynku, kodPocztowy));
							}
						}
					}

					if (ObiektySportowe != null && ObiektySportowe.Count > 0)
					{
						//Tworzenie pierwszego pustego rekordu
						var possibleSportFacilityList = new List<object>
						{
							new { Id=-9, Name="" },
							new { Id="", Name="Brak obiektu sportowego" }
						};

						//Dodanie aktualnych osób w bazie do listy
						possibleSportFacilityList.AddRange(ObiektySportowe.Select(os => new
						{
							Id = os.IdObiektSportowy,
							Name = $"{os.Nazwa}, {os.Miejscowosc}, {os.Ulica}, {os.NumerBudynku}, {os.KodPocztowy}"
						}));

						// Create the SelectList
						ViewData["possibleSportFacilityList"] = new SelectList(
							possibleSportFacilityList,
							"Id",
							"Name"
						);
					}
					else
					{
						ViewData["possibleSportFacilityList"] = new SelectList(new List<object>
						{
							new { Id=-9, Name="" },
							new { Id="", Name="Brak obiektu sportowego" }
						},
						"Id",
						"Name");
					}

					string getPossibleTreatFacilities = "SELECT IdLokalGastronomiczny, Nazwa, Miejscowosc, Ulica, NumerBudynku, KodPocztowy FROM LokalGastronomiczny";

					using (SqlCommand command = new SqlCommand(getPossibleTreatFacilities, connection))
					{
						using (SqlDataReader reader = command.ExecuteReader())
						{
							LokaleGastronomiczne = new List<LokalGastronomiczny>();
							int idLokalGastronomiczny;
							string nazwa;
							string miejscowosc;
							string ulica;
							string numerBudynku;
							string kodPocztowy;
							while (reader.Read())
							{
								idLokalGastronomiczny = reader.GetInt32(0);
								nazwa = reader.GetString(1);
								miejscowosc = reader.GetString(2);
								ulica = reader.GetString(3);
								numerBudynku = reader.GetString(4);
								kodPocztowy = reader.GetString(5);
								LokaleGastronomiczne.Add(new LokalGastronomiczny(idLokalGastronomiczny, nazwa, miejscowosc, ulica, numerBudynku, kodPocztowy));
							}
						}
					}

					if (LokaleGastronomiczne != null && LokaleGastronomiczne.Count > 0)
					{
						//Tworzenie pierwszego pustego rekordu
						var possibleTreatFacilityList = new List<object>
						{
							new { Id=-9, Name="" },
							new { Id="", Name="Brak lokalu gastronomicznego" }
						};

						//Dodanie aktualnych osób w bazie do listy
						possibleTreatFacilityList.AddRange(LokaleGastronomiczne.Select(lg => new
						{
							Id = lg.IdLokalGastronomiczny,
							Name = $"{lg.Nazwa}, {lg.Miejscowosc}, {lg.Ulica}, {lg.NumerBudynku}, {lg.KodPocztowy}"
						}));

						// Create the SelectList
						ViewData["possibleTreatFacilityList"] = new SelectList(
							possibleTreatFacilityList,
							"Id",
							"Name"
						);
					}
					else
					{
						ViewData["possibleTreatFacilityList"] = new SelectList(new List<object>
						{
							new { Id=-9, Name="" },
							new { Id="", Name="Brak lokalu gastronomicznego" }
						},
						"Id",
						"Name");
					}

					string getPossibleSponsors = "SELECT IdSponsor, Nazwa, Miejscowosc, Ulica, NumerBudynku, KodPocztowy FROM Sponsor";

					using (SqlCommand command = new SqlCommand(getPossibleSponsors, connection))
					{
						using (SqlDataReader reader = command.ExecuteReader())
						{
							Sponsorzy = new List<Sponsor>();
							int idSponsor;
							string nazwa;
							string miejscowosc;
							string ulica;
							string numerBudynku;
							string kodPocztowy;
							while (reader.Read())
							{
								idSponsor = reader.GetInt32(0);
								nazwa = reader.GetString(1);
								miejscowosc = reader.GetString(2);
								ulica = reader.GetString(3);
								numerBudynku = reader.GetString(4);
								kodPocztowy = reader.GetString(5);
								Sponsorzy.Add(new Sponsor(idSponsor, nazwa, miejscowosc, ulica, numerBudynku, kodPocztowy));
							}
						}
					}

					if (Sponsorzy != null && Sponsorzy.Count > 0)
					{
						//Tworzenie pierwszego pustego rekordu
						var possibleSponsorList = new List<object>
						{
							new { Id=-9, Name="" },
							new { Id="", Name="Brak sponsora" }
						};

						//Dodanie aktualnych osób w bazie do listy
						possibleSponsorList.AddRange(Sponsorzy.Select(s => new
						{
							Id = s.IdSponsor,
							Name = $"{s.Nazwa}, {s.Miejscowosc}, {s.Ulica}, {s.NumerBudynku}, {s.KodPocztowy}"
						}));

						// Create the SelectList
						ViewData["possibleSponsorList"] = new SelectList(
							possibleSponsorList,
							"Id",
							"Name"
						);
					}
					else
					{
						ViewData["possibleSponsorList"] = new SelectList(new List<object>
						{
							new { Id=-9, Name="" },
							new { Id="", Name="Brak sponsora" }
						},
						"Id",
						"Name");
					}

					//pobieramy jsona z sesji
					string json2 = HttpContext.Session.GetString("LoggedUser");
					//deserializujemy jsona do obiektu o nazwie loggedUser, ktory bedzie zawiera³ informacje o aktualnie zalogowanym u¿ytkowniku
					Osoba loggedUser2 = JsonSerializer.Deserialize<Osoba>(json2);

					string getFunctionsThatUserIsDoing = "SELECT IdFunkcja FROM OsobaOdpowiedzialna WHERE OsobaOdpowiedzialna.IdWydarzenieSportowe = @idWydarzenia AND OsobaOdpowiedzialna.IdOsoba = @idOsoby";

					List<int>? idFunkcji = new List<int>();

					using (SqlCommand command = new SqlCommand(getFunctionsThatUserIsDoing, connection))
					{
						command.Parameters.AddWithValue("@idWydarzenia", id);
						command.Parameters.AddWithValue("@idOsoby", loggedUser2.IdOsoba);
						using (SqlDataReader reader = command.ExecuteReader())
						{
							int idFunkcja;
							while (reader.Read())
							{
								idFunkcja = reader.GetInt32(0);
								idFunkcji.Add(idFunkcja);
							}
						}
					}

					foreach (var idFunkcja in idFunkcji)
					{
						switch (idFunkcja)
						{
							case 1:
								string getCurrentlySetSportFacilityForDiscipline = "SELECT ObiektSportowy.Nazwa FROM ObiektSportowy INNER JOIN Zawody ON ObiektSportowy.IdObiektSportowy = Zawody.IdObiektSportowy INNER JOIN WydarzenieSportowe ON Zawody.IdWydarzenieSportowe = WydarzenieSportowe.IdWydarzenieSportowe WHERE WydarzenieSportowe.IdWydarzenieSportowe = @idWydarzenia AND Zawody.IdDyscyplina = @IdDyscyplina";

								AktualneObiektySportowe = new List<AktualnyObiektSportowy?>();

								for (int i = 0; i < DyscyplinyOrazDaty.Count; i++)
								{
									using (SqlCommand command = new SqlCommand(getCurrentlySetSportFacilityForDiscipline, connection))
									{
										command.Parameters.AddWithValue("@idWydarzenia", id);
										command.Parameters.AddWithValue("@IdDyscyplina", DyscyplinyOrazDaty.ElementAt(i).IdDyscyplina);
										string? facilityName = (string)command.ExecuteScalar();
										if (facilityName == null)
										{
											AktualneObiektySportowe.Add(null);
										}
										else
										{
											AktualneObiektySportowe.Add(new AktualnyObiektSportowy(facilityName));
										}
									}
								}
								break;

							case 2:
								string getCurrentlySetTreatFacilityForDiscpiline = "SELECT LokalGastronomiczny.Nazwa FROM LokalGastronomiczny INNER JOIN Poczestunek ON LokalGastronomiczny.IdLokalGastronomiczny = Poczestunek.IdLokalGastronomiczny INNER JOIN Zawody ON Poczestunek.IdZawody = Zawody.IdZawody WHERE Zawody.IdWydarzenieSportowe = @idWydarzenia AND Zawody.IdDyscyplina = @IdDyscyplina";

								AktualneLokaleGastronomiczne = new List<AktualnyLokalGastronomiczny?>();

								for (int i = 0; i < DyscyplinyOrazDaty.Count; i++)
								{
									using (SqlCommand command = new SqlCommand(getCurrentlySetTreatFacilityForDiscpiline, connection))
									{
										command.Parameters.AddWithValue("idWydarzenia", id);
										command.Parameters.AddWithValue("@IdDyscyplina", DyscyplinyOrazDaty.ElementAt(i).IdDyscyplina);
										string? facilityName = (string)command.ExecuteScalar();
										if (facilityName == null)
										{
											AktualneLokaleGastronomiczne.Add(null);
										}
										else
										{
											AktualneLokaleGastronomiczne.Add(new AktualnyLokalGastronomiczny(facilityName));
										}
									}
								}
								break;

							case 3:
								string getCurrentlySetSponsorForDiscpiline = "SELECT Sponsor.Nazwa FROM Sponsor INNER JOIN SponsorZawodow ON Sponsor.IdSponsor = SponsorZawodow.IdSponsor INNER JOIN Zawody ON SponsorZawodow.IdZawody = Zawody.IdZawody WHERE Zawody.IdWydarzenieSportowe = @idWydarzenia AND Zawody.IdDyscyplina = @IdDyscyplina";

								AktualniSponsorzy = new List<AktualnySponsor?>();

								for (int i = 0; i < DyscyplinyOrazDaty.Count; i++)
								{
									using (SqlCommand command = new SqlCommand(getCurrentlySetSponsorForDiscpiline, connection))
									{
										command.Parameters.AddWithValue("idWydarzenia", id);
										command.Parameters.AddWithValue("@IdDyscyplina", DyscyplinyOrazDaty.ElementAt(i).IdDyscyplina);
										string? sponsorName = (string)command.ExecuteScalar();
										if (sponsorName == null)
										{
											AktualniSponsorzy.Add(null);
										}
										else
										{
											AktualniSponsorzy.Add(new AktualnySponsor(sponsorName));
										}
									}
								}
								break;
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
