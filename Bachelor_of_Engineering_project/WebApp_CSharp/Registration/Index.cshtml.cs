using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages
{
    public class RegistrationModel : PageModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdOsoba { get; set; }
        public string Login { get; set; }
        public string Haslo { get; set; }
        public string PowtorzoneHaslo { get; set; }
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        public string Email { get; set; }
        public DateTime DataUrodzenia { get; set; }
        public string NumerTelefonu { get; set; }

        public string errorMessage = "";

        public void OnGet()
        {
            var isLoggedIn = HttpContext.Session.GetString("IsLoggedIn");
            if (isLoggedIn == "true")
            {
                Response.Redirect("/IndexForLoggedUser");
            }
        }

        public void OnPost()
        {
            RegistrationModel uzytkownikDoRejestracji = new RegistrationModel();

            uzytkownikDoRejestracji.Login = Request.Form["login"];
            uzytkownikDoRejestracji.Haslo = Request.Form["haslo"];
            uzytkownikDoRejestracji.PowtorzoneHaslo = Request.Form["powtorzHaslo"];
            uzytkownikDoRejestracji.Imie = Request.Form["imie"];
            uzytkownikDoRejestracji.Nazwisko = Request.Form["nazwisko"];
            uzytkownikDoRejestracji.Email = Request.Form["email"];
            uzytkownikDoRejestracji.NumerTelefonu = Request.Form["numerTelefonu"];

            if (uzytkownikDoRejestracji.Login.Length == 0 || uzytkownikDoRejestracji.Haslo.Length == 0 ||
                uzytkownikDoRejestracji.PowtorzoneHaslo.Length == 0 || uzytkownikDoRejestracji.Imie.Length == 0 ||
                uzytkownikDoRejestracji.Nazwisko.Length == 0 || uzytkownikDoRejestracji.Email.Length == 0 ||
                uzytkownikDoRejestracji.NumerTelefonu.Length == 0)
            {
                errorMessage = "Wszystkie pola musz¹ zostaæ wype³nione!";
                return;
            }


            if (!uzytkownikDoRejestracji.Haslo.Equals(uzytkownikDoRejestracji.PowtorzoneHaslo))
            {
                errorMessage = "Has³a nie s¹ takie same!";
                return;
            }

            try
            {
                string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string emailRepeatCheck = "SELECT COUNT (*) FROM Osoba WHERE Email=@Email;";
                    int counterEmails = 0;

                    using (SqlCommand command = new SqlCommand(emailRepeatCheck, connection))
                    {
                        command.Parameters.AddWithValue("@Email", uzytkownikDoRejestracji.Email);
                        counterEmails = (int)command.ExecuteScalar();
                    }

                    if (counterEmails > 0)
                    {
                        errorMessage = "U¿ytkownik o takim adresie Email ju¿ istnieje w bazie danych!";
                        connection.Close();
                        return;
                    }

                    string loginRepeatCheck = "SELECT COUNT (*) FROM LoginData WHERE Username=@Login;";
                    int counterLogin = 0;

                    using (SqlCommand command = new SqlCommand(loginRepeatCheck, connection))
                    {
                        command.Parameters.AddWithValue("@Login", uzytkownikDoRejestracji.Login);
                        counterLogin = (int)command.ExecuteScalar();
                    }

                    if (counterLogin > 0)
                    {
                        errorMessage = "U¿ytkownik o takiej nazwie u¿ytkownika ju¿ istnieje w bazie danych!";
                        connection.Close();
                        return;
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }

            if (DateTime.TryParse(Request.Form["dataUrodzenia"], out DateTime dataUrodzenia))
            {
                uzytkownikDoRejestracji.DataUrodzenia = dataUrodzenia;
            }
            else
            {
                errorMessage = "Podano nieprawid³owy format daty!";
                return;
            }


            //Je¿eli nie ma pustych pól to dodajemy u¿ytkownika do bazy (z zaszyfrowanym has³em)
            try
            {
                string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    HasloHasher hasher = new HasloHasher();
                    string hashedHaslo = hasher.Hash(uzytkownikDoRejestracji.Haslo);


                    //W zmiennej insertToOsoba s¹ dwie kwerendy. Pierwsza dodaje dane do kolumny Osoba, druga po niej siê wykonuj¹ca zwraca ostatnio dodan¹ wartoœæ Id,
                    //która zosta³a dodana w tym samym zakresie kwerendy. W tym przypadku zwróci IdOsoba, które jest automatycznie generowane
                    //podczas INSERT i dalej bêdzie ono dodawane w nastêpnej kwerendzie (dodania danych do tabeli LoginData)
                    string insertToOsoba = "INSERT INTO Osoba (Imie, Nazwisko, Email, DataUrodzenia, NumerTelefonu) VALUES (@Imie, @Nazwisko, @Email, @DataUrodzenia, @NumerTelefonu); SELECT SCOPE_IDENTITY()";

                    string insertToLoginData = "INSERT INTO LoginData (IdOsoba, Username, Password) VALUES (@IdOsoba, @Login, @Haslo);";

                    using (SqlCommand command = new SqlCommand(insertToOsoba, connection))
                    {
                        command.Parameters.AddWithValue("@Imie", uzytkownikDoRejestracji.Imie);
                        command.Parameters.AddWithValue("@Nazwisko", uzytkownikDoRejestracji.Nazwisko);
                        command.Parameters.AddWithValue("@Email", uzytkownikDoRejestracji.Email);
                        command.Parameters.AddWithValue("@DataUrodzenia", uzytkownikDoRejestracji.DataUrodzenia);
                        command.Parameters.AddWithValue("@NumerTelefonu", uzytkownikDoRejestracji.NumerTelefonu);

                        //Wykona kwerende i zwróci pierwsz¹ wartoœæ pierwszej kolumny pierwszego wiersza.
                        //W tym przypadku zwróci dane z kwerendy 'SELECT SCOPE_IDENTITY()' i w tym przypadku bêdzie to 
                        //wartosc IdOsoba osoby, której dane w³aœnie zosta³y dodane do tabeli Osoba
                        object wynik = command.ExecuteScalar();


                        //Je¿eli rezutlat wykonania kwerendy nie jest pusty
                        //to konwertuje go na Int32 i dodaje tak do w³aœciwoœci IdOsoba obiektu uzytkownikDoRejestracji
                        //czyli wartoœæ IdOsoba osoby, która w³aœnie zosta³a dodana do tablicy Osoba zostanie w niej zapisana.
                        //Je¿eli nie to przypisuje polu errorMessage now¹ wartoœæ i wraca z tej funkcji (dodanie danych nie powiod³o siê)
                        if (wynik != null)
                        {
                            uzytkownikDoRejestracji.IdOsoba = Convert.ToInt32(wynik);
                        }
                        else
                        {
                            errorMessage = "Nie uda³o siê uzyskaæ wartoœci pola IdOsoba z tabeli Osoba";
                            connection.Close();
                            return;
                        }
                    }

                    using (SqlCommand command = new SqlCommand(insertToLoginData, connection))
                    {
                        //Dodajemy do LoginData dane logowania osoby o podanym Id (utworzonego w poprzedniej kwerendzie)
                        command.Parameters.AddWithValue("@IdOsoba", uzytkownikDoRejestracji.IdOsoba);
                        command.Parameters.AddWithValue("@Login", uzytkownikDoRejestracji.Login);
                        command.Parameters.AddWithValue("@Haslo", hashedHaslo);

                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return;
            }



            //Czysczenie danych z uzytkownika ktory zosta³ dodany do bazy
            uzytkownikDoRejestracji.IdOsoba = 0;
            uzytkownikDoRejestracji.Login = "";
            uzytkownikDoRejestracji.Haslo = "";
            uzytkownikDoRejestracji.Imie = "";
            uzytkownikDoRejestracji.Nazwisko = "";
            uzytkownikDoRejestracji.Email = "";
            uzytkownikDoRejestracji.DataUrodzenia = DateTime.Now;
            uzytkownikDoRejestracji.NumerTelefonu = "";

            //TempData bo bêdziemy to przekazywaæ do IndexModel
            TempData["SuccessMessage"] = "U¿ytkownik zosta³ zarejestrowany pomyœlnie!";

            Response.Redirect("/IndexForNotLoggedUser");
        }
    }
}
