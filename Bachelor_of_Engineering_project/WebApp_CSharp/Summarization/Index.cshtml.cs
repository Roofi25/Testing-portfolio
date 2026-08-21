using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.Inne;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.DeletePlayers;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.Summarization
{
    public class IndexModel : PageModel
    {
		public WydarzenieSportowe WydarzenieSportowe { get; set; }
		public IList<DyscyplinaOrazData> DyscyplinyOrazDaty { get; set; }
		public Osoba Organizator { get; set; }
		public IList<Osoba> PomocnicyWydarzenia { get; set; }
		public IList<Funkcja> Funkcje { get; set; }
		public IList<ObiektSportowy> ObiektySportowe { get; set; }
		public IList<LokalGastronomiczny> LokaleGastronomiczne { get; set; }
		public IList<Sponsor> Sponsorzy { get; set; }
		public IList<Zawody> Zawody { get; set; }
		public IList<int> IdZawodow { get; set; } = new List<int>();
		public IList<ZawodnikDoWyswietlenia> Zawodnicy { get; set; }
		public IList<int> WybraneZawody { get; set; }

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
							int idDyscyplina;
							string nazwaDyscypliny;
							DateTime dataZawodow;
							while (reader.Read())
							{
								idDyscyplina = reader.GetInt32(0);
								nazwaDyscypliny = reader.GetString(1);
								dataZawodow = reader.GetDateTime(2);
								DyscyplinyOrazDaty.Add(new DyscyplinaOrazData(idDyscyplina, nazwaDyscypliny, dataZawodow)); //wykorzystanie konstruktora klasy ni¿ej
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

					string getPomocnicyWydarzenia = "SELECT Osoba.IdOsoba, Imie, Nazwisko, Email, DataUrodzenia, NumerTelefonu FROM Osoba INNER JOIN OsobaOdpowiedzialna ON Osoba.IdOsoba = OsobaOdpowiedzialna.IdOsoba WHERE OsobaOdpowiedzialna.IdWydarzenieSportowe = @id";

					using(SqlCommand command = new SqlCommand(getPomocnicyWydarzenia, connection))
					{
						command.Parameters.AddWithValue("@id", id);

						using(SqlDataReader reader = command.ExecuteReader())
						{
							PomocnicyWydarzenia = new List<Osoba>();
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
								PomocnicyWydarzenia.Add(new Osoba(idOsoba, imie, nazwisko, email, dataUrodzenia, numerTelefonu));
							}
						}
					}

					string getFunkcje = "SELECT IdFunkcja, Nazwa, Opis FROM Funkcja";

					using(SqlCommand command = new SqlCommand(getFunkcje, connection))
					{
						using(SqlDataReader reader = command.ExecuteReader())
						{
							Funkcje = new List<Funkcja>();
							int idFunkcja;
							string nazwa;
							string opis;
							while(reader.Read())
							{
								idFunkcja = reader.GetInt32(0);
								nazwa = reader.GetString(1);
								opis = reader.GetString(2);
								Funkcje.Add(new Funkcja(idFunkcja, nazwa, opis));
							}
						}
					}

					string getObiektySportoweDlaWydarzenia = "SELECT ObiektSportowy.IdObiektSportowy, Nazwa, Miejscowosc, Ulica, NumerBudynku, KodPocztowy FROM ObiektSportowy INNER JOIN Zawody ON ObiektSportowy.IdObiektSportowy = Zawody.IdObiektSportowy WHERE Zawody.IdWydarzenieSportowe = @id";

					using (SqlCommand command = new SqlCommand(getObiektySportoweDlaWydarzenia, connection))
					{
						command.Parameters.AddWithValue("@id", id);
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

					string getLokaleGastronomiczneDlaWydarzenia = "SELECT DISTINCT LokalGastronomiczny.IdLokalGastronomiczny, Nazwa, Miejscowosc, Ulica, NumerBudynku, KodPocztowy FROM LokalGastronomiczny INNER JOIN Poczestunek ON LokalGastronomiczny.IdLokalGastronomiczny = Poczestunek.IdLokalGastronomiczny INNER JOIN Zawody ON Poczestunek.IdZawody = Zawody.IdZawody WHERE Zawody.IdWydarzenieSportowe = @id";

					using (SqlCommand command = new SqlCommand(getLokaleGastronomiczneDlaWydarzenia, connection))
					{
						command.Parameters.AddWithValue("@id", id);
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

					string getSponsorzyDlaWydarzenia = "SELECT Sponsor.IdSponsor, Nazwa, Miejscowosc, Ulica, NumerBudynku, KodPocztowy FROM Sponsor INNER JOIN SponsorZawodow ON Sponsor.IdSponsor = SponsorZawodow.IdSponsor INNER JOIN Zawody ON SponsorZawodow.IdZawody = Zawody.IdZawody WHERE Zawody.IdWydarzenieSportowe = @id";

					using (SqlCommand command = new SqlCommand(getSponsorzyDlaWydarzenia, connection))
					{
						command.Parameters.AddWithValue("@id", id);
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

					for (int i = 0; i < Zawody.Count; i++)
					{
						IdZawodow.Add(Zawody.ElementAt(i).IdZawody);
					}

					string getPlayersInfo = "SELECT DISTINCT Osoba.IdOsoba, Imie, Nazwisko, Email, DataUrodzenia, NumerTelefonu FROM Osoba INNER JOIN Zawodnik ON Osoba.IdOsoba = Zawodnik.IdOsoba INNER JOIN Zawody ON Zawodnik.IdZawody = Zawody.IdZawody WHERE Zawody.IdWydarzenieSportowe = @id";

					using (SqlCommand command = new SqlCommand(getPlayersInfo, connection))
					{
						command.Parameters.AddWithValue("@id", id);
						using (SqlDataReader reader = command.ExecuteReader())
						{
							Zawodnicy = new List<ZawodnikDoWyswietlenia>();
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
								numerTelefonu = reader.GetString(5); ;
								Zawodnicy.Add(new ZawodnikDoWyswietlenia(idOsoba, imie, nazwisko, email, dataUrodzenia, numerTelefonu));
							}
						}
					}

					string getPlayerWybraneZawody = "SELECT Zawodnik.IdZawody FROM Zawodnik INNER JOIN Zawody ON Zawodnik.IdZawody = Zawody.IdZawody INNER JOIN WydarzenieSportowe ON Zawody.IdWydarzenieSportowe = WydarzenieSportowe.IdWydarzenieSportowe WHERE Zawodnik.IdOsoba = @id";

					List<bool> bools = new List<bool>();

					for (int i = 0; i < Zawodnicy.Count; i++)
					{
						using (SqlCommand command2 = new SqlCommand(getPlayerWybraneZawody, connection))
						{
							command2.Parameters.AddWithValue("@id", Zawodnicy.ElementAt(i).IdOsoba);

							using (SqlDataReader reader2 = command2.ExecuteReader())
							{
								WybraneZawody = new List<int>();
								int idZawody;
								while (reader2.Read())
								{
									idZawody = reader2.GetInt32(0);
									WybraneZawody.Add(idZawody);
								}
							}
							bools = new List<bool>();
							for (int j = 0; j < IdZawodow.Count; j++)
							{
								if (WybraneZawody.Contains(IdZawodow.ElementAt(j)))
								{
									bools.Add(true);
								}
								else
								{
									bools.Add(false);
								}
							}
						}
						Zawodnicy.ElementAt(i).CzyZapisanyNaZawody = bools;
						Zawodnicy.ElementAt(i).WybraneZawody = WybraneZawody;
					}

					connection.Close();
				}
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = ex.Message;
				Response.Redirect("/Index");
			}
		}
    }
}
