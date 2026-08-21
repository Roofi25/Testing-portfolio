using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System_wspomagajacy_organizacje_wydarzen_sportowych.Models.BazaDanych;

namespace System_wspomagajacy_organizacje_wydarzen_sportowych.Pages
{
    public class LoginModel : PageModel
    {
        public int IdOsoba { get; set; }
        public string Login { get; set; }
        public string Haslo { get; set; }

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
            LoginModel uzytkownikDoZalogowania = new LoginModel();

            uzytkownikDoZalogowania.Login = Request.Form["login"];
            uzytkownikDoZalogowania.Haslo = Request.Form["haslo"];

            if (string.IsNullOrEmpty(uzytkownikDoZalogowania.Login) || string.IsNullOrEmpty(uzytkownikDoZalogowania.Haslo) || uzytkownikDoZalogowania.Login == "" || uzytkownikDoZalogowania.Haslo == "")
            {
                TempData["ErrorMessage"] = "Wszystkie pola musz¹ zostaæ wype³nione!";
                Response.Redirect("/Login");
            }

            string connectionString = "Server=DESKTOP-6139JC9;Database=SystemWspomagajacyDB;Trusted_Connection=True;TrustServerCertificate=True";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    //sprawdzamy czy istnieje uzytkownik o podanym login'ie
                    string loginChecked = "SELECT COUNT (*) FROM LoginData WHERE Username=@Login;";
                    int counterLogin = 0;

                    using (SqlCommand command = new SqlCommand(loginChecked, connection))
                    {
                        //zastêpowanie argumentów wartoœciami Login podanymi przez u¿ytkownika
                        command.Parameters.AddWithValue("@Login", uzytkownikDoZalogowania.Login);
                        //wykonanie kwerendy i przypisanie iloœci zwróconych rekordów do zmiennej
                        //je¿eli wyjdzie inna liczba ni¿ 1 to nie zalogujemy siê poprawnie.
                        counterLogin = (int)command.ExecuteScalar();
                    }

                    if (counterLogin != 1)
                    {
                        TempData["ErrorMessage"] = "Nieprawid³owa nazwa u¿ytkownika lub has³o!";
                        connection.Close();
                        return;
					}

                    //po sprawdzeniu czy uzytkownik o takiej nazwie znajduje siê w bazie danych
                    //sprawdzamy czy wprowadzone has³o jest prawid³owe u¿ywaj¹c metody VerifyHaslo z klasy HasloHasher
                    HasloHasher hasher = new HasloHasher();

                    //zwraca zahashowane has³o uzytkownika z bazy
                    string getHashedHaslo = "SELECT Password from LoginData WHERE Username=@Login";
                    string hashedHasloFromDB;

                    using (SqlCommand command = new SqlCommand(getHashedHaslo, connection))
                    {
                        //zmienia nazwe u¿ytkownika w kwerendzie na tak¹ któr¹ poda³ u¿ytkownik w formularzu
                        command.Parameters.AddWithValue("@Login", uzytkownikDoZalogowania.Login);
                        //zapisuje to has³o w tej zmiennej
                        hashedHasloFromDB = (string)command.ExecuteScalar();
                    }

                    //je¿eli has³a nie bêd¹ siê zgadzaæ to nie zalogujemy siê poprawnie
                    if (!hasher.VerifyHaslo(hashedHasloFromDB, uzytkownikDoZalogowania.Haslo))
                    {
                        TempData["ErrorMessage"] = "Nieprawid³owa nazwa u¿ytkownika lub has³o!";
                        connection.Close();
                        return;
					}
                    //czyœci zmienn¹ z zahashowanym has³em w przypadku poprawnego logowania
                    hashedHasloFromDB = "";

                    //Walidacja przebieg³a pomyœlnie - Mo¿emy zapisaæ dane osobowe uzytkownika do danych sesji
                    //Pobranie danych z bazy i wrzucenie ich do HttpContext.Session
                    string getUserData = "SELECT Osoba.IdOsoba, Imie, Nazwisko, Email, DataUrodzenia, NumerTelefonu FROM Osoba INNER JOIN LoginData ON Osoba.IdOsoba = LoginData.IdOsoba WHERE Username=@Login";

                    using (SqlCommand command = new SqlCommand(getUserData, connection))
                    {
                        command.Parameters.AddWithValue("@Login", uzytkownikDoZalogowania.Login);
                        Osoba osobaZalogowana = new Osoba();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                osobaZalogowana.IdOsoba = Convert.ToInt32(reader["IdOsoba"]);
                                osobaZalogowana.Imie = reader["Imie"].ToString();
                                osobaZalogowana.Nazwisko = reader["Nazwisko"].ToString();
                                osobaZalogowana.Email = reader["Email"].ToString();
                                osobaZalogowana.DataUrodzenia = Convert.ToDateTime(reader["DataUrodzenia"]);
                                osobaZalogowana.NumerTelefonu = reader["NumerTelefonu"].ToString();
                            }
                        }
                        //Przechowujemy osobê z danymi pobranymi z bazy w json
                        string json = JsonSerializer.Serialize(osobaZalogowana);

                        //Przechowujemy tego jsona z danymi osoby zalogowanej w HttpContext.Session
                        //oraz zmieniamy flage czy jest zalogowany na true
                        HttpContext.Session.SetString("LoggedUser", json);
                        HttpContext.Session.SetString("IsLoggedIn", "true");
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return;
			}

            //Czyszczenie danych u¿ytkownika po udanym logowaniu
            uzytkownikDoZalogowania.IdOsoba = 0;
            uzytkownikDoZalogowania.Login = "";
            uzytkownikDoZalogowania.Haslo = "";

            //TempData bo bêdziemy to przekazywaæ do IndexModel
            TempData["SuccessMessage"] = "Zalogowano pomyœlnie!";

			Response.Redirect("/Index");

		}
    }
}
