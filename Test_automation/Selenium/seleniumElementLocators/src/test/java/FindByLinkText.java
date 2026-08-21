import io.github.bonigarcia.wdm.WebDriverManager;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chrome.ChromeDriver;

public class FindByLinkText
{
    public static void main(String [] args) throws InterruptedException {
        WebDriverManager.chromedriver().setup();
        WebDriver driver = new ChromeDriver();

        driver.get("https://the-internet.herokuapp.com");
        //locating element by link text
        /*
        //gets the element by the linkText locator and clicks it
        //the value is what's inside the tag (NOT THE VALUE OF HREF ATTRIBUTE!)
        driver.findElement(By.linkText("Form Authentication")).click();
        driver.findElement(By.id("username")).sendKeys("tomsmith");
        driver.findElement(By.name("password")).sendKeys("SuperSecretPassword!");
        //driver.findElement(By.id("login")).submit();
        driver.findElement(By.className("radius")).click();
        */

        //locating element by partial link text
        //if there are more links that have this partial text like in this example (Auth)
        //then it will go and click on the first one it will find (the highest in the html code)
        //driver.findElement(By.partialLinkText("Auth")).click();

        driver.findElement(By.partialLinkText("JavaScript onload")).click();
        Thread.sleep(1000);
        driver.navigate().back();
        Thread.sleep(2000);
        driver.quit();
    }
}
