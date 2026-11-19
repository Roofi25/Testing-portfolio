import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { ProductListingPage } from '../pages/ProductListingPage';
import { ProductDetailPage } from '../pages/ProductDetailPage';

test.describe("Product Sorting", () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        const loginPage = new LoginPage(page);
        await loginPage.login("standard_user", "secret_sauce");
    })

    test('[SLT-15] Verify that customer is able to sort products by price from lowest to highest', async ({ page }) => {
        const productListingPage = new ProductListingPage(page);
        // Sorting the items on the website by price from lowest to highest
        await productListingPage.sortProductsBy('lohi');
        // Getting the actual prices from the website after sorting
        const actualPrices = await productListingPage.getAllProductPrices();
        // Creating a copy of the prices table and sorting it ascending again
        const expectedPrices = [...actualPrices].sort((a, b) => a - b);
        // If both of these tables are equal to each other, the test passes
        await expect(actualPrices).toEqual(expectedPrices);
    })
})
