using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Reflection.PortableExecutable;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.Inne;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages
{
    public class MoreInfoModel : PageModel
    {
        public WydarzenieSportowe? WydarzenieSportowe { get; set; }
        public IList<DyscyplinaOrazData>? DyscyplinyOrazDaty { get; set; }
        public Osoba? Organizator { get; set; }

        public void OnGet(int id)
        {
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            if(isLoggedIn != "true")
            {
                Response.Redirect("/Index");
            }

            HttpContext.Session.SetString("id", id.ToString());
            WydarzenieSportowe = new WydarzenieSportowe(id, "", "", "");

            string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

            try
            {
                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string getSportEvent = "SELECT Ogloszenie FROM WydarzenieSportowe WHERE IdWydarzenieSportowe=@id";

                    using(SqlCommand command = new SqlCommand(getSportEvent, connection))
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

                    using(SqlCommand command = new SqlCommand(getOrganisatorId, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        string? organisatorId = command.ExecuteScalar().ToString();

                        if(organisatorId == null )
                        {
                            organisatorId = "";
                        }

                        HttpContext.Session.SetString("organisatorId", organisatorId);

                        if(organisatorId != null )
                        {
                            string getOrganisator = "SELECT IdOsoba, Imie, Nazwisko, Email, DataUrodzenia, NumerTelefonu from Osoba WHERE IdOsoba = @id";

                            using(SqlCommand _command = new SqlCommand(getOrganisator, connection))
                            {
                                _command.Parameters.AddWithValue("@id", organisatorId);

                                using(SqlDataReader reader = _command.ExecuteReader())
                                {
                                    int idOsoba;
                                    string imie;
                                    string nazwisko;
                                    string email;
                                    DateTime dataUrodzenia;
                                    string numerTelefonu;
                                    while(reader.Read())
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

					connection.Close();
                }
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                Response.Redirect("/Index");
            }
        }
    }
}
