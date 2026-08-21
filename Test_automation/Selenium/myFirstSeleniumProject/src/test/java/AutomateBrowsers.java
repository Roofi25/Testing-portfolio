import io.github.bonigarcia.wdm.WebDriverManager;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.edge.EdgeDriver;
import org.openqa.selenium.firefox.FirefoxDriver;
import org.openqa.selenium.safari.SafariDriver;

public class AutomateBrowsers
{
    public static void main(String[] args) {
        //We need to have browser installed obviously to make automated testing work

        //WebDriverManager.firefoxdriver().setup();
        //WebDriver driver = new FirefoxDriver();
        //WebDriverManager.edgedriver().setup();
        //WebDriver driver = new EdgeDriver();

        //With safari on MAC we need to allow making remote automation in the "develop" tab.
        //This is the case with every Apple product.
        //If you can't see 'Develop' menu then we need to click on safari tab
        //and click on preferences and then in Advanced tab we need to check the "Show Develop menu in menu bar" there
        WebDriverManager.safaridriver().setup();
        WebDriver driver = new SafariDriver();


    }
}
