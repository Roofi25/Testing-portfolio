using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.Inne;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.DeletePlayers
{
    public class IndexModel : PageModel
    {
		public WydarzenieSportowe? WydarzenieSportowe { get; set; }
		public IList<DyscyplinaOrazData>? DyscyplinyOrazDaty { get; set; } //klasa znajduje siê w pliku Index.cshtml.cs w folderze MoreInfo
		public Osoba? Organizator { get; set; }
		public IList<Zawody> Zawody { get; set; }
		public IList<int> IdZawodow { get; set; } = new List<int>();
		public IList<ZawodnikDoWyswietlenia>? ZawodnicyDoWyswietlenia { get; set; }
		public IList<int> WybraneZawody { get; set; }

		public void OnGet(int id)
		{
            HttpContext.Session.Remove("idZawodnik");
            HttpContext.Session.Remove("idWydarzenieSportowe");
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

					for(int i = 0; i < Zawody.Count; i++)
					{
						IdZawodow.Add(Zawody.ElementAt(i).IdZawody);
					}

					string getPlayersInfo = "SELECT DISTINCT Osoba.IdOsoba, Imie, Nazwisko, Email, DataUrodzenia, NumerTelefonu FROM Osoba INNER JOIN Zawodnik ON Osoba.IdOsoba = Zawodnik.IdOsoba INNER JOIN Zawody ON Zawodnik.IdZawody = Zawody.IdZawody WHERE Zawody.IdWydarzenieSportowe = @id";

					using (SqlCommand command = new SqlCommand(getPlayersInfo, connection))
					{
						command.Parameters.AddWithValue("@id", id);
						using (SqlDataReader reader = command.ExecuteReader())
						{
							ZawodnicyDoWyswietlenia = new List<ZawodnikDoWyswietlenia>();
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
								ZawodnicyDoWyswietlenia.Add(new ZawodnikDoWyswietlenia(idOsoba, imie, nazwisko, email, dataUrodzenia, numerTelefonu));
							}
						}
					}

					string getPlayerWybraneZawody = "SELECT Zawodnik.IdZawody FROM Zawodnik INNER JOIN Zawody ON Zawodnik.IdZawody = Zawody.IdZawody INNER JOIN WydarzenieSportowe ON Zawody.IdWydarzenieSportowe = WydarzenieSportowe.IdWydarzenieSportowe WHERE Zawodnik.IdOsoba = @id";

					List<bool> bools = new List<bool>();

					for (int i = 0; i < ZawodnicyDoWyswietlenia.Count; i++)
					{
						using (SqlCommand command2 = new SqlCommand(getPlayerWybraneZawody, connection))
						{
							command2.Parameters.AddWithValue("@id", ZawodnicyDoWyswietlenia.ElementAt(i).IdOsoba);

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
								if(WybraneZawody.Contains(IdZawodow.ElementAt(j)))
								{
									bools.Add(true);
								}
								else
								{
									bools.Add(false);
								}
							}
						}
						ZawodnicyDoWyswietlenia.ElementAt(i).CzyZapisanyNaZawody = bools;
						ZawodnicyDoWyswietlenia.ElementAt(i).WybraneZawody = WybraneZawody;
					}

					HttpContext.Session.SetString("akcja", "usun¹æ");
					 
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
