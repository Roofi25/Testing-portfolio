import io.github.bonigarcia.wdm.WebDriverManager;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chrome.ChromeDriver;

public class MyFirstTestClass
{
    public static void main(String[] args) throws InterruptedException {
        WebDriverManager.chromedriver().setup();
        WebDriver driver = new ChromeDriver();
        driver.manage().window().maximize();
        driver.get("https://www.google.com");
        String googleWindowHandle = driver.getWindowHandle();
        //Window handle is a code that is unique to the page.
        //It is useful while testing (assertions) on exact page and we want to make sure we are testing correct page.
        //No two pages have the same codes, even from the same website.
        System.out.println("Google window handle is: " + googleWindowHandle);
        String googlePageSource = driver.getPageSource();
        System.out.println("Google page source is: " + googlePageSource);
        String googleTitle = driver.getTitle();
        System.out.println("Google title is: " + googleTitle);
        String googleURL = driver.getCurrentUrl();
        System.out.println("Google URL is: " + googleURL);
        driver.navigate().to("https://www.udemy.com");
        String udemyPageSource = driver.getPageSource();
        System.out.println("Udemy page source is: " + udemyPageSource);
        String udemyTitle = driver.getTitle();
        System.out.println("Udemy title is: " + udemyTitle);
        String udemyURL = driver.getCurrentUrl();
        System.out.println("Udemy URL is: " + udemyURL);
        driver.navigate().back();
        googleURL = driver.getCurrentUrl();
        System.out.println("Google URL is: " + googleURL);
        driver.manage().window().fullscreen();
        driver.navigate().refresh();
        driver.navigate().forward();
        udemyURL = driver.getCurrentUrl();
        System.out.println("Udemy URL is: " + udemyURL);
        driver.quit(); //Closes the entire driver
        driver.close(); //Closes only the currect window on the driver

    }
}
