import { test, expect } from '@playwright/test';
import { ProductListingPage } from '../pages/ProductListingPage';
import { LoginPage } from '../pages/LoginPage';

test.describe("Product browsing", () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        const loginPage = new LoginPage(page);
        await loginPage.login('standard_user', 'secret_sauce');
    })

    test("[SLT-12] Verify that customer is able to navigate to the product details page from the product listing page by clicking onto the product's name", async({ page }) => {
        const productListingPage = new ProductListingPage(page);
        await productListingPage.clickOnTheProductName();

        await expect(page).toHaveURL('https://www.saucedemo.com/inventory-item.html?id=4');
    })

    test("[SLT-13] Verify that customer is able to navigate to the product details page from the product listing page by clicking onto the product's image", async({ page }) => { 
        await page.pause();
        const productListingPage = new ProductListingPage(page);
        await productListingPage.clickOnTheProductImage();

        await expect(page).toHaveURL('https://www.saucedemo.com/inventory-item.html?id=4');
    })
})