using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages.AddTheEvent2
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string Nazwa { get; set; } = null!;
        [BindProperty]
        public string Ogloszenie { get; set; } = null!;
        [BindProperty]
        public int LiczbaZawodow { get; set; }
        [BindProperty]
        public List<DateTime> Daty { get; set; } = new List<DateTime>();
        public List<Dyscyplina>? Dyscypliny { get; set; }
        [BindProperty]
        public List<int> WybraneDyscyplinyZListy { get; set; } = new List<int>();
        [BindProperty]
        public List<string>? NazwyDyscyplinWpisanych { get; set; } = new List<string>();
        [BindProperty]
        public IFormFile? Logo { get; set; }
        public IList<Zawody> Zawody { get; set; }

        public void OnGet()
        {
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            if (isLoggedIn != "true")
            {
                Response.Redirect("/Index");
            }
            else
            {
                var nazwaWydarzenia = HttpContext.Session.GetString("NazwaWydarzenia");
                var ogloszenieWydarzenia = HttpContext.Session.GetString("OgloszenieWydarzenia");
                var liczbaZawodowWydarzenia = HttpContext.Session.GetString("LiczbaZawodowWydarzenia");
                if (nazwaWydarzenia == null || ogloszenieWydarzenia == null || liczbaZawodowWydarzenia == null)
                {
                    TempData["ErrorMessage"] = "Nazwa wydarzenia, opis wydarzenia, b¹dz liczba zawodów wydarzenia by³a pusta!";
                    Response.Redirect("/Index");
                }
                Nazwa = nazwaWydarzenia;
                Ogloszenie = ogloszenieWydarzenia;
                LiczbaZawodow = int.Parse(liczbaZawodowWydarzenia);

                Daty = new List<DateTime>(new DateTime[LiczbaZawodow]);
                WybraneDyscyplinyZListy = new List<int>(new int[LiczbaZawodow]);
                NazwyDyscyplinWpisanych = new List<string>(new string[LiczbaZawodow]);

                string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

                try
                {
                    using(SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string getDyscypliny = "SELECT IdDyscyplina, Nazwa FROM Dyscyplina";

                        using (SqlCommand command = new SqlCommand(getDyscypliny, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                Dyscypliny = new List<Dyscyplina>();
                                int idDyscypliny;
                                string nazwa;
                                while (reader.Read())
                                {
                                    idDyscypliny = reader.GetInt32(0);
                                    nazwa = reader.GetString(1);
                                    Dyscypliny.Add(new Dyscyplina(idDyscypliny, nazwa));
                                }
                            }
                        }

                        var mozliweDyscyplinyDoWyboru = new List<object>()
                        {
                            new { Id=0, Name=""}
                        };

                        mozliweDyscyplinyDoWyboru.AddRange(Dyscypliny.Select(d => new
                        {
                            Id = d.IdDyscyplina,
                            Name = d.Nazwa
                        }));

                        // Create the SelectList
                        ViewData["mozliweDyscyplinyDoWyboru"] = new SelectList(
                            mozliweDyscyplinyDoWyboru,
                            "Id",
                            "Name"
                        );

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

        public IActionResult OnPost()
        {
            if (Logo != null && Logo.Length > 0)
            {
                if(WybraneDyscyplinyZListy == null || WybraneDyscyplinyZListy.Count == 0)
                {
                    for (int i = 0; i < LiczbaZawodow; i++)
                    {
                        WybraneDyscyplinyZListy.Add(0);
                    }
                }

                IList<string> NazwyDyscyplinWBazie;

                string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string getNazwyDyscyplin = "SELECT Nazwa FROM Dyscyplina";

                        using (SqlCommand command = new SqlCommand(getNazwyDyscyplin, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                NazwyDyscyplinWBazie = new List<string>();
                                string nazwa;
                                while (reader.Read())
                                { 
                                    nazwa = reader.GetString(0);
                                    NazwyDyscyplinWBazie.Add(nazwa);
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return RedirectToPage();
                }

                Daty.Clear();

                for (int i = 0; i < LiczbaZawodow; i++)
                {
                    if(WybraneDyscyplinyZListy.ElementAt(i) > 0 && NazwyDyscyplinWpisanych.ElementAt(i) != null)
                    {
                        TempData["ErrorMessage"] = $"Zawody nr {i + 1}: Mo¿esz albo wybraæ dyscyplinê z listy, albo dodaæ now¹!";
                        return RedirectToPage();
                    }

                    if(WybraneDyscyplinyZListy.ElementAt(i) == 0 && NazwyDyscyplinWpisanych.ElementAt(i) == null)
                    {
                        TempData["ErrorMessage"] = $"Zawody nr {i + 1}: Dyscyplina nie zosta³a wybrana!";
                        return RedirectToPage();
                    }

                    if(NazwyDyscyplinWBazie.Contains(NazwyDyscyplinWpisanych.ElementAt(i)))
                    {
                        TempData["ErrorMessage"] = $"Zawody nr {i + 1}: Wpisana dyscyplina znajduje siê ju¿ na liœcie!";
                        return RedirectToPage();
                    }

                    var data = Request.Form[$"Daty[{i}]"];

                    if (DateTime.TryParse(data, out DateTime Data))
                    {
                        if (Data < DateTime.Now.AddDays(14))
                        {
                            TempData["ErrorMessage"] = $"Data zawodów nr {i + 1} musi byæ co najmniej dwa tygodnie po dzisiejszej dacie!";
                            return RedirectToPage();
                        }
                        Daty.Add(Data);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Format daty zawodów nr {i + 1} nie jest prawid³owy!";
                        return RedirectToPage();
                    }
                }

                for(int i = 1; i < Daty.Count; i++)
                {
                    if(Daty.ElementAt(i) <= Daty.ElementAt(i-1))
                    {
                        TempData["ErrorMessage"] = "Zawody musz¹ odbywaæ siê w porz¹dku chronologicznym z przynajmniej jednym dniem odstêpu!";
                        return RedirectToPage();
                    }
                }

                if (!ModelState.IsValid)
                {
                    return RedirectToPage();
                }

                try
                {
                    using(SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        int IdWydarzenia;

                        string addSportEvent = "INSERT INTO WydarzenieSportowe(IdOrganizator, Nazwa, Ogloszenie, Logo) VALUES(@idOrganizator, @nazwa, @ogloszenie, @logo);SELECT SCOPE_IDENTITY()";

                        using(SqlCommand command = new SqlCommand(addSportEvent, connection))
                        {
                            string json = HttpContext.Session.GetString("LoggedUser");
                            Osoba loggedUser = JsonSerializer.Deserialize<Osoba>(json);

                            command.Parameters.AddWithValue("@idOrganizator", loggedUser.IdOsoba);
                            command.Parameters.AddWithValue("@nazwa", Nazwa);
                            command.Parameters.AddWithValue("@ogloszenie", Ogloszenie);

                            var nazwaPliku = Path.GetFileName(Logo.FileName);
                            var sciezkaDoPliku = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logos", nazwaPliku);

                            command.Parameters.AddWithValue("@logo", nazwaPliku);

                            object wynik = command.ExecuteScalar();

                            if (wynik != null)
                            {
                                IdWydarzenia = Convert.ToInt32(wynik);
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Nie uda³o siê uzyskaæ wartoœci pola IdWydarzenieSportowe z tabeli WydarzenieSportowe";
                                connection.Close();
                                return RedirectToPage();
                            }

                            using (var stream = new FileStream(sciezkaDoPliku, FileMode.Create))
                            {
                                Logo.CopyTo(stream);
                            }
                        }

                        int IdDyscypliny;

                        string addZawody = "INSERT INTO Zawody(IdDyscyplina, IdWydarzenieSportowe, IdObiektSportowy, Data) VALUES(@idDyscyplina, @idWydarzenieSportowe, @idObiektSportowy, @data)";
                        string addDyscyplina = "INSERT INTO Dyscyplina(Nazwa) VALUES(@nazwa);SELECT SCOPE_IDENTITY()";

                        for(int i = 0; i < LiczbaZawodow; i++)
                        {
                            using(SqlCommand command = new SqlCommand(addZawody, connection))
                            {
                                if(WybraneDyscyplinyZListy.ElementAt(i) != 0)
                                {
                                    command.Parameters.AddWithValue("@idDyscyplina", WybraneDyscyplinyZListy.ElementAt(i));
                                    command.Parameters.AddWithValue("@idWydarzenieSportowe", IdWydarzenia);
                                    command.Parameters.AddWithValue("idObiektSportowy", DBNull.Value);
                                    command.Parameters.AddWithValue("@data", Daty.ElementAt(i));

                                    command.ExecuteNonQuery();
                                }
                                else
                                {
                                    using(SqlCommand command2 = new SqlCommand(addDyscyplina, connection))
                                    {
                                        command2.Parameters.AddWithValue("@nazwa", NazwyDyscyplinWpisanych.ElementAt(i));

                                        object wynik = command2.ExecuteScalar();

                                        if (wynik != null)
                                        {
                                            IdDyscypliny = Convert.ToInt32(wynik);
                                        }
                                        else
                                        {
                                            TempData["ErrorMessage"] = "Nie uda³o siê uzyskaæ wartoœci pola IdDyscyplina z tabeli Dyscyplina";
                                            connection.Close();
                                            return RedirectToPage();
                                        }
                                    }
                                    command.Parameters.AddWithValue("@idDyscyplina", IdDyscypliny);
                                    command.Parameters.AddWithValue("@idWydarzenieSportowe", IdWydarzenia);
                                    command.Parameters.AddWithValue("idObiektSportowy", DBNull.Value);
                                    command.Parameters.AddWithValue("@data", Daty.ElementAt(i));

                                    command.ExecuteNonQuery();
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
                                DateTime data;
                                while (reader.Read())
                                {
                                    idZawody = reader.GetInt32(0);
                                    data = reader.GetDateTime(1);
                                    Zawody.Add(new Zawody(idZawody, data));
                                }
                            }
                        }

                        string createNullOsobaOdpowiedzialna = "INSERT INTO OsobaOdpowiedzialna(IdOsoba, IdWydarzenieSportowe, IdFunkcja) VALUES (@idOsoby, @idWydarzenia, @idFunkcji)";

                        for (int i = 1; i <= 4; i++)
                        {
                            using (SqlCommand command = new SqlCommand(createNullOsobaOdpowiedzialna, connection))
                            {
                                command.Parameters.AddWithValue("@idOsoby", DBNull.Value);
                                command.Parameters.AddWithValue("@IdWydarzenia", IdWydarzenia);
                                command.Parameters.AddWithValue("@idFunkcji", i);
                                command.ExecuteScalar();
                            }
                        }

                        string createNullLokalGastronomiczny = "INSERT INTO Poczestunek(IdLokalGastronomiczny, IdZawody) VALUES (@idLokaluGastronomicznego, @idZawodow)";

                        for (int i = 0; i < LiczbaZawodow; i++)
                        {
                            using (SqlCommand command = new SqlCommand(createNullLokalGastronomiczny, connection))
                            {
                                command.Parameters.AddWithValue("@idLokaluGastronomicznego", DBNull.Value);
                                command.Parameters.AddWithValue("@idZawodow", Zawody.ElementAt(i).IdZawody);
                                command.ExecuteScalar();
                            }
                        }

                        string createNullSponsor = "INSERT INTO SponsorZawodow(IdSponsor, IdZawody) VALUES (@idSponsora, @idZawodow)";

                        for (int i = 0; i < LiczbaZawodow; i++)
                        {
                            using (SqlCommand command = new SqlCommand(createNullSponsor, connection))
                            {
                                command.Parameters.AddWithValue("@idSponsora", DBNull.Value);
                                command.Parameters.AddWithValue("@idZawodow", Zawody.ElementAt(i).IdZawody);
                                command.ExecuteScalar();
                            }
                        }

                        connection.Close();

                        TempData["SuccessMessage"] = "Uda³o Ci siê utworzyæ wydarzenie!";
                        return RedirectToPage("/IndexForLoggedUser/Index");
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return RedirectToPage();
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Musisz dodaæ logo wydarzenia sportowego!";
                return RedirectToPage();
            }
        }
    }
}
