using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.Confirm
{
    public class IndexModel : PageModel
    {
		public List<int> WybraneZawody { get; set; } = new List<int>();
		public void OnGet(int id, string? WybraneZawody)
        {
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            if (isLoggedIn != "true")
            {
                Response.Redirect("/Index");
            }

            HttpContext.Session.SetInt32("idZawodnik", id);
            if(WybraneZawody != null)
            {
				HttpContext.Session.SetString("WybraneZawody", WybraneZawody);
			}

            try
            {
                string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";
                
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string getEventId = "SELECT IdWydarzenieSportowe FROM OsobaOdpowiedzialna WHERE IdOsoba = @id";


                    using(SqlCommand command = new SqlCommand(getEventId, connection))
                    {
                        //pobieramy jsona z sesji
                        string json = HttpContext.Session.GetString("LoggedUser");
                        //deserializujemy jsona do obiektu o nazwie loggedUser, ktory bedzie zawiera³ informacje o aktualnie zalogowanym u¿ytkowniku
                        Osoba loggedUser = JsonSerializer.Deserialize<Osoba>(json);

                        command.Parameters.AddWithValue("@id", loggedUser.IdOsoba);

                        HttpContext.Session.SetInt32("idWydarzenieSportowe", (int)command.ExecuteScalar());
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                Response.Redirect("/HelpingTheEvent?id="+HttpContext.Session.GetInt32("idWydarzenieSportowe").ToString());
            }
        }

        public void OnPost()
        {
            try
            {
                string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    if(HttpContext.Session.GetString("akcja").Equals("usun¹æ"))
                    {
                        string deletePlayerFromEvent = "DELETE FROM Zawodnik WHERE IdOsoba = @id";

                        using (SqlCommand command = new SqlCommand(deletePlayerFromEvent, connection))
                        {
                            command.Parameters.AddWithValue("@id", HttpContext.Session.GetInt32("idZawodnik"));

                            command.ExecuteNonQuery();
                        }

                        string deletePlayerAsOsoba = "DELETE FROM Osoba WHERE IdOsoba = @id";

                        using(SqlCommand command = new SqlCommand(deletePlayerAsOsoba, connection))
                        {
                            command.Parameters.AddWithValue("@id", HttpContext.Session.GetInt32("idZawodnik"));

                            command.ExecuteNonQuery();
                        }

						TempData["SuccessMessage"] = "Pomyœlnie usuniêto zawodnika z danego wydarzenia sportowego!";

						Response.Redirect("/DeletePlayers?id=" + HttpContext.Session.GetInt32("idWydarzenieSportowe").ToString());
					}
                    else if(HttpContext.Session.GetString("akcja").Equals("edytowaæ"))
                    {
                        int? idWydarzenia = HttpContext.Session.GetInt32("idWydarzenieSportowe");
                        int? idOsoby = HttpContext.Session.GetInt32("idZawodnik");

						string imie = Request.Form["imie"];
						string nazwisko = Request.Form["nazwisko"];
						string email = Request.Form["email"];
						if (DateTime.TryParse(Request.Form["dataUrodzenia"], out DateTime dataUrodzenia)) { }
						else
						{
							TempData["ErrorMessage"] = "Podano nieprawid³owy format daty!";
							Response.Redirect("/HelpingTheEvent?id=" + HttpContext.Session.GetInt32("idWydarzenieSportowe").ToString());
						}
						string numerTelefonu = Request.Form["numerTelefonu"];

						if (imie.IsNullOrEmpty() || nazwisko.IsNullOrEmpty() || email.IsNullOrEmpty() || numerTelefonu.IsNullOrEmpty())
						{
							TempData["ErrorMessage"] = "Wszystkie pola musz¹ zostaæ wype³nione!";
							Response.Redirect("/HelpingTheEvent?id=" + HttpContext.Session.GetInt32("idWydarzenieSportowe").ToString());
						}

                        string[] selectedCompetitions = Request.Form["WybraneZawody"].ToArray();

                        if(selectedCompetitions.Length == 0)
                        {
							TempData["ErrorMessage"] = "Zawodnik musi byæ zapisany przynajmniej na jedne zawody!";
							Response.Redirect("/HelpingTheEvent?id=" + HttpContext.Session.GetInt32("idWydarzenieSportowe").ToString());
						}

                        List<int> WybraneZawody = new List<int>();

                        foreach(string competitionStrId in selectedCompetitions)
                        {
                            if(int.TryParse(competitionStrId, out int competitionId))
                            {
                                WybraneZawody.Add(competitionId);
                            }
                        }

                        WybraneZawody = WybraneZawody;

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

                        string deletePlayerCurrentRecords = "DELETE FROM Zawodnik INNER JOIN Zawody ON Zawodnik.IdZawody = Zawody.IdZawody INNER JOIN WydarzenieSportowe ON Zawody.IdWydarzenieSportowe = WydarzenieSportowe.IdWydarzenieSportowe WHERE Zawodnik.IdOsoba = @idOsoby AND WydarzenieSportowe.IdWydarzenieSportowe = @idWydarzenia";

                        using(SqlCommand command = new SqlCommand(deletePlayerCurrentRecords, connection))
                        {
                            command.Parameters.AddWithValue("@idOsoby", idOsoby);
                            command.Parameters.AddWithValue("@idWydarzenia", idWydarzenia);

                            command.ExecuteNonQuery();
                        }

                        string addUpdatedPlayerRecords = "INSERT INTO Zawodnik(IdZawody, IdOsoba) VALUES(@idZawodow, i@dOsoby)";

                        for(int i = 0; i < WybraneZawody.Count; i++)
                        {
							using (SqlCommand command = new SqlCommand(addUpdatedPlayerRecords, connection))
							{
                                command.Parameters.AddWithValue("@idZawodow", WybraneZawody.ElementAt(i));
                                command.Parameters.AddWithValue("@idOsoby", idOsoby);
                                command.ExecuteNonQuery();
							}
						}
					}

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                Response.Redirect("/HelpingTheEvent?id="+HttpContext.Session.GetInt32("idWydarzenieSportowe").ToString());
            }
        }
    }
}
