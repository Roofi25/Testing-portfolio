package pages;

import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

import java.time.Duration;

public class LoginPage
{
    private WebDriver driver;
    private By usernameInput = By.id("user-name");
    private By passwordInput = By.id("password");
    private By loginButton = By.id("login-button");

    public LoginPage(WebDriver driver)
    {
        this.driver = driver;
    }

    public void login(String username, String password)
    {
        driver.findElement(usernameInput).sendKeys(username);
        driver.findElement(passwordInput).sendKeys(password);

        // Explicit wait of 5 seconds or less (if the login button becomes clickable before 5th second then it proceeds with the test and doesn't wait)
        WebDriverWait webDriverWait = new WebDriverWait(driver, Duration.ofSeconds(5));
        webDriverWait.until(ExpectedConditions.elementToBeClickable(loginButton)).click();
    }

    public boolean isLoginButtonVisible()
    {
        return driver.findElement(loginButton).isDisplayed();
    }
}
