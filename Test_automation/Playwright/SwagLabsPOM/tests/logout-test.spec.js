import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { ProductListingPage } from '../pages/ProductListingPage';

test.describe("Logout", () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
    })

    test('[SLT-3] Verify that user can log out', async ({ page }) => {
        const loginPage = new LoginPage(page);
        await loginPage.login("standard_user", "secret_sauce");

        const productListingPage = new ProductListingPage(page);
        await productListingPage.logout();

        await expect(loginPage.loginButton, 'Login button is not visible!').toBeVisible();
    })
});