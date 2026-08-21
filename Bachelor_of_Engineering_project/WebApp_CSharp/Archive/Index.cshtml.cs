using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages
{
    public class ArchiveModel : PageModel
    {
        public IList<WydarzenieSportowe>? WydarzeniaSportowe { get; set; }
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
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string getSportsEvents = "SELECT DISTINCT WydarzenieSportowe.IdWydarzenieSportowe, Nazwa, Ogloszenie, Logo FROM WydarzenieSportowe INNER JOIN Zawody ON WydarzenieSportowe.IdWydarzenieSportowe = Zawody.IdWydarzenieSportowe WHERE Zawody.Data <= @data";

                    using (SqlCommand command = new SqlCommand(getSportsEvents, connection))
                    {
                        command.Parameters.AddWithValue("@data", DateTime.Now);
                        //odczytuje od pierwszego do ostatniego wiersza wyniki komendy getSportsEvents
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            WydarzeniaSportowe = new List<WydarzenieSportowe>();
                            int idWydarzenia;
                            string nazwa;
                            string ogloszenie;
                            string logo;
                            //tworzymy liste wydarzen sportowych, sk³adaj¹cych siê z idWydarzeniaSportowego oraz jego ogloszenia i logo
                            //pomijamy idOrganizatora, gdy¿ jest to klucz obcy, który sam bêdzie móg³ byæ dodany przez u¿ytkownika systemu
                            while (reader.Read())
                            {
                                idWydarzenia = reader.GetInt32(0);
                                nazwa = reader.GetString(1);
                                ogloszenie = reader.GetString(2);
                                logo = reader.GetString(3);
                                WydarzeniaSportowe.Add(new WydarzenieSportowe(idWydarzenia, nazwa, ogloszenie, logo)); //wykorzystanie konstruktora
                            }
                        }
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
