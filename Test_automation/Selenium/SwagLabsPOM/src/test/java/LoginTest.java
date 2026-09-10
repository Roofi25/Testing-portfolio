import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chrome.ChromeDriver;
import org.openqa.selenium.chrome.ChromeOptions;
import org.testng.Assert;
import org.testng.annotations.*;
import pages.InventoryPage;
import pages.LoginPage;

import java.util.Map;

//import java.time.Duration;

public class LoginTest
{
    // This below is only if we want one driver, but it wont work in parallel execution
    //private WebDriver driver;

    // This is needed for parallel execution. Parallel execution is set up in testng.xml
    // Now instead of writing driver.get or driver.manage we write driver.get().get or driver.get().manage itd.
    // When we call get() we are calling the specific instance of a driver since it's parallel so it can be multiple of them depending on the number of threads
    // we are using
    private ThreadLocal<WebDriver> driver = new ThreadLocal<>();

    // BeforeTest means only once and then it executes all methods
    // BeforeMethod means that it executes what is inside a setup() before all of the methods
    @BeforeMethod
    public void setup()
    {
        // Headless mode
        ChromeOptions chromeOptions = new ChromeOptions();
        chromeOptions.addArguments("--headless=new");
        chromeOptions.addArguments("--window-size=1920,1080");

        // Incognito mode, because on normal mode the safety "password leaked" pop out was blocking the hamburger icon
        chromeOptions.addArguments("--incognito");
        chromeOptions.addArguments("--disable-popup-blocking");

        // Turning off the popups about saving and leaking passwords (it was blocking the hamburger icon)
        chromeOptions.setExperimentalOption("prefs", Map.of(
                "credentials_enable_service", false,
                "profile.password_manager_enabled", false,
                "safebrowsing.enabled", false
        ));

        driver.set(new ChromeDriver(chromeOptions));
        driver.get().get("https://www.saucedemo.com");

        // If element is not visible, then it waits up to maximum 5 seconds. If the element appears before 5th second then it proceeds with the test.
        // This is implicit wait and it shouldn't be used if there is explicit wait used.
        //driver.get().manage().timeouts().implicitlyWait(Duration.ofSeconds(5));
    }

    // Logging in with valid credentials
    @Test
    public void LoginWithValidCredentials() {
        LoginPage loginPage = new LoginPage(this.driver.get());
        loginPage.login("standard_user", "secret_sauce");

        InventoryPage inventoryPage = new InventoryPage(this.driver.get());
        Assert.assertTrue(inventoryPage.isCartLinkVisible());
    }

    // Correct logout (technically it should have been in different Test class (not LoginTest)
    @Test
    public void CorrectLogout() throws InterruptedException {
        // First we need to log in (all tests should be hermetic and independent of other tests
        LoginPage loginPage = new LoginPage(this.driver.get());
        loginPage.login("standard_user", "secret_sauce");

        InventoryPage inventoryPage = new InventoryPage(this.driver.get());
        inventoryPage.logout();

        Assert.assertTrue(loginPage.isLoginButtonVisible());
    }

    // No RAM leak
    @AfterMethod
    public void cleanup()
    {
        if(driver.get() != null)
        {
            driver.get().quit();
            driver.remove();
        }
    }
}
