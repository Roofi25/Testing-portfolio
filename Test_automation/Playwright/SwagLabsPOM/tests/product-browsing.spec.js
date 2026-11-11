import { test, expect } from '@playwright/test';
import { ProductListingPage } from '../pages/ProductListingPage';
import { LoginPage } from '../pages/LoginPage';
import { ProductDetailPage } from '../pages/ProductDetailPage';

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
        const productListingPage = new ProductListingPage(page);
        await productListingPage.clickOnTheProductImage();

        await expect(page).toHaveURL('https://www.saucedemo.com/inventory-item.html?id=4');
    })

    test("[SLT-14] Verify that data integrity works correctly between product listing page and product detail page", async({ page }) => {
        const productListingPage = new ProductListingPage(page);
        const productNamePLP = await productListingPage.itemName.innerText();
        const productDescriptionPLP = await productListingPage.itemDescription.innerText();
        const productPricePLP = (await productListingPage.itemPrice.innerText()).replace(/[^0-9.]/g, '');
        const productImagePLP = await productListingPage.itemImage.getAttribute('src');
        await productListingPage.clickOnTheProductName();

        await expect(page).toHaveURL('https://www.saucedemo.com/inventory-item.html?id=4');

        const productDetailPage = new ProductDetailPage(page);
        const productNamePDP = await productDetailPage.itemName.innerText();
        const productDescriptionPDP = await productDetailPage.itemDescription.innerText();
        const productPricePDP = (await productDetailPage.itemPrice.innerText()).replace(/[^0-9.]/g, '');
        const productImagePDP = await productDetailPage.itemImage.getAttribute('src');

        expect(productNamePLP).toEqual(productNamePDP);
        expect(productDescriptionPLP).toEqual(productDescriptionPDP);
        expect(productPricePLP).toEqual(productPricePDP);
        expect(productImagePLP).toEqual(productImagePDP);
    })
})