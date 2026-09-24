import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';

test.describe('Session Management', () => {
    test('[SLT-4] Verify that unauthorized user cannot access the inventory page', async ({ page }) => {
        const loginPage = new LoginPage(page);

        await page.goto('https://www.saucedemo.com/inventory.html');

        await expect(page).toHaveURL('https://www.saucedemo.com/')
        await expect(loginPage.errorMessage).toContainText("Epic sadface: You can only access '/inventory.html'");
    })
});