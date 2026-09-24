import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { InventoryPage } from '../pages/InventoryPage';

test.describe("Logout", () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
    })

    test('[SLT-3] Verify that user can log out', async ({ page }) => {
        const loginPage = new LoginPage(page);
        await loginPage.login("standard_user", "secret_sauce");

        const inventoryPage = new InventoryPage(page);
        await inventoryPage.logout();

        await expect(loginPage.loginButton, 'Login button is not visible!').toBeVisible();
    })
});