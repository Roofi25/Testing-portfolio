import io.github.bonigarcia.wdm.WebDriverManager;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chrome.ChromeDriver;

public class FindByIdAndByNameAndByClassName
{
    public static void main(String[] args) throws InterruptedException {
        WebDriverManager.chromedriver().setup();
        WebDriver driver = new ChromeDriver();

        driver.get("https://the-internet.herokuapp.com/login");

        //locating element by id
        /*
        //sendKeys -> send sequence of keys on a keyboard
        driver.findElement(By.id("username")).sendKeys("tomsmith");
        //sometimes there are auto generated ids that change after
        //page refresh. Usually these are sequences of random numbers as value
        driver.findElement(By.id("password")).sendKeys("SuperSecretPassword!");
        //submit -> sumbits the form
        driver.findElement(By.id("login")).submit();
        //alternative - clicking a button (doesn't have an id, just class)
        //driver.findElement(By.className("radius")).click();
        //locating by id is preferred
        */

        //locating element by name
        /*
        //sendKeys -> send sequence of keys on a keyboard
        driver.findElement(By.name("username")).sendKeys("tomsmith");
        driver.findElement(By.name("password")).sendKeys("SuperSecretPassword!");
        //submit -> sumbits the form
        driver.findElement(By.id("login")).submit();
        //alternative - clicking a button (doesn't have an id, just class)
        //driver.findElement(By.className("radius")).click();
        //locating by name is preferred

        //locating element by className
        */
        //compund class so it will return error (compund class -> class name with spaces)
        //driver.findElement(By.className("large-6 small-12 columns")).sendKeys("TEST");
        //it has to be a signle word like this:
        //driver.findElement(By.className("large-6")).sendKeys("TEST");
        //but it will still return error in this case, because there are more
        //tags with this class name on the page
        //locating by className is not preferred (not unique enough)

        Thread.sleep(2000);
        driver.quit();
    }
}
