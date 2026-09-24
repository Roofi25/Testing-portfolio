import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';

test.describe('Session Management', () => {
    test('[SLT-4] Verify that unauthorized user cannot access the inventory page', async ({ page }) => {
        const loginPage = new LoginPage(page);

        await page.goto('https://www.saucedemo.com/inventory.html');
        
        // Unauthorized user shouldn't be able to access the inventory page. The app should redirect the user that has not an active session to the login page 
        // and also display the error message.
        await expect(page).toHaveURL('https://www.saucedemo.com/')
        await expect(loginPage.errorMessage).toContainText("Epic sadface: You can only access '/inventory.html'");
    })

    // Normally I should use api calls to register and login during every new test run. It saves time + it makes sure that 
    // tests are atomic and have one responsibility.
    test('[SLT-5] Verify that authorized user cannot access the login page', async ({ page }) => {
        const loginPage = new LoginPage(page);

        await page.goto('https://www.saucedemo.com/')
        await loginPage.login('standard_user', 'secret_sauce');

        // Authorized user shouldn't be able to access the login page. The app should redirect the user that has an active session to the inventory page.
        await page.goto('https://www.saucedemo.com/');
        await expect(page).toHaveURL('https://www.saucedemo.com/inventory.html');
    })
});