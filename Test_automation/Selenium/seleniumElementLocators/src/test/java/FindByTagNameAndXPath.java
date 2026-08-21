import io.github.bonigarcia.wdm.WebDriverManager;
import org.openqa.selenium.By;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.chrome.ChromeDriver;
import org.openqa.selenium.WebDriver;

import java.util.List;

public class FindByTagNameAndXPath
{
    public static void main(String[] args) throws InterruptedException {
        WebDriverManager.chromedriver().setup();
        WebDriver driver = new ChromeDriver();

        driver.get("https://the-internet.herokuapp.com/");

        //locating elements by tagName
        /*
        driver.findElement(By.partialLinkText("Form Authentication")).click();
        driver.findElement(By.id("username")).sendKeys("tomsmith");
        driver.findElement(By.name("password")).sendKeys("SuperSecretPassword!");
        //if there is only one element of this tag it's fine
        driver.findElement(By.tagName("Button")).click();
        */

        driver.findElement(By.linkText("Sortable Data Tables")).click();
        //we are initialising the found element (of id "table") to the
        //variable of type WebElement
        WebElement table = driver.findElement(By.id("table1"));
        //we are initialising the found elements (of tag "tr") to the
        //list of variables of type WebElement
        List<WebElement> tableRows = table.findElements(By.tagName("tr"));
        System.out.println(tableRows.get(1).getText());
        System.out.println(tableRows.get(2).getText());
        System.out.println(tableRows.get(3).getText());
        System.out.println(tableRows.get(4).getText());
        //we are initialising the found elements (of tag "td" in the 2nd row in table) to the
        //list of variables of type WebElement
        List<WebElement> secondRowTableDataCells = tableRows.get(1).findElements(By.tagName("td"));
        System.out.println(secondRowTableDataCells.get(1).getText());
        driver.navigate().back();
        driver.findElement(By.linkText("Form Authentication")).click();


        //locating elements by XPath
        //relative XPath - (//)driver searches in the whole page without needing to provide the absolute path
        //absolute XPath - (/html/body/div[1])driver searches in the whole page when you input a full path (this example it's the 1st div that is on the page - the highest in the html code)
        driver.findElement(By.xpath("//input[@name='username']")).sendKeys("tomsmith");
        driver.findElement(By.xpath("//input[@id='password']")).sendKeys("SuperSecretPassword!");
        driver.findElement(By.tagName("button")).click();

        Thread.sleep(2000);
        driver.quit();
    }
}
