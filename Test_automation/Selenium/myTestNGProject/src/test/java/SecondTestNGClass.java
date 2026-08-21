import io.github.bonigarcia.wdm.WebDriverManager;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chrome.ChromeDriver;
import org.testng.annotations.AfterTest;
import org.testng.annotations.BeforeTest;
import org.testng.annotations.Test;

public class SecondTestNGClass
{
    //Driver needs to be in the class
    //for all of the methods to accesss it below
    WebDriver driver;

    //This will be done before all of the tests
    @BeforeTest
    public void prepare() throws InterruptedException {
        WebDriverManager.chromedriver().setup();
        driver = new ChromeDriver();
        Thread.sleep(3000);
    }

    //This will be done after all of the tests
    @AfterTest
    public void finish()
    {
        driver.quit();
    }

    //The highest priority is 0.
    //The lower the number the higher the priority.
    //It can also be a negative number like -2
    @Test (priority = 0)
    public void openWebsite() throws InterruptedException {
        System.out.println("Let's open the browser");
        driver.navigate().to("https://twitter.com");
        Thread.sleep(3000);
    }

    @Test (priority = 1)
    public void signUp()
    {
        System.out.println("Signing up");
    }

    @Test (priority = 2)
    public void logIn()
    {
        System.out.println("Login");
    }

    @Test (priority = 3)
    public void addToCart()
    {
        System.out.println("Add items to cart");
    }

    @Test (priority = 4)
    public void logOut()
    {
        System.out.println("Logging out");
    }
}
