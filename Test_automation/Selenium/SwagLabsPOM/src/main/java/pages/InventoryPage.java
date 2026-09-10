package pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.time.Duration;

public class InventoryPage
{
    private WebDriver driver;
    private By cartLink = By.cssSelector("[data-test='shopping-cart-link']");
    private By hamburgerIcon = By.id("react-burger-menu-btn");
    private By logoutLink = By.cssSelector("[data-test='logout-sidebar-link']");

    public InventoryPage(WebDriver driver)
    {
        this.driver = driver;
    }

    public boolean isCartLinkVisible()
    {
        return driver.findElement(cartLink).isDisplayed();
    }

    public void logout() throws InterruptedException {
        WebDriverWait webDriverWait = new WebDriverWait(driver, Duration.ofSeconds(5));

        // Explicit wait of 5 seconds or less (if the hamburger icon link becomes clickable before 5th second then it proceeds with the test and doesn't wait)
        webDriverWait.until(ExpectedConditions.elementToBeClickable(hamburgerIcon)).click();

        // Explicit wait of 5 seconds or less (if the logout link becomes clickable before 5th second then it proceeds with the test and doesn't wait)
        webDriverWait.until(ExpectedConditions.elementToBeClickable(logoutLink)).click();
    }
}
