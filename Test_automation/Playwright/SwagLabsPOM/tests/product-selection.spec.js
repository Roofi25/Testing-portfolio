import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { ProductListingPage } from '../pages/ProductListingPage';
import { ProductDetailPage } from '../pages/ProductDetailPage';

test.describe("Product Selection", () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        const loginPage = new LoginPage(page);
        await loginPage.login("standard_user", "secret_sauce");
    })

    test('[SLT-6] Verify that customer is able to add product to the cart directly from the inventory page', async ({ page }) => {
        const productListingPage = new ProductListingPage(page);
        const isitemCounterVisible = await productListingPage.cartItemCounter.isVisible();
        const initialItemCount = isitemCounterVisible ? parseInt(await productListingPage.cartItemCounter.innerText(), 10) : 0;
        await productListingPage.addProductToCart();
        const expectedItemCount = (initialItemCount + 1).toString();

        await expect(productListingPage.removeFromCartButton).toHaveText('Remove');
        await expect(productListingPage.cartItemCounter).toHaveText(expectedItemCount);
    })

    // I can also modify local storage through Playwright so I start the test with the product already added and I just remove it.
    // It's better to do so, but in this small project I decided to prepare the state (adding product to cart) via UI in the removing product from cart test itself
    test('[SLT-7] Verify that customer is able to remove product from the cart directly from the inventory page', async ({ page }) => {
        const productListingPage = new ProductListingPage(page);
        await productListingPage.addProductToCart();

        await productListingPage.removeProductFromCart();

        await expect(productListingPage.addToCartButton).toHaveText('Add to cart');

        // All of the test runs are starting from a clean state so we know that after adding and removing an item from the cart, the cart item count will be 0
        // so we are asserting the lack of cart item counter (the badge with the number of items in the cart should not be visible)
        await expect(productListingPage.cartItemCounter).toBeHidden();
    })

    test('[SLT-8] Verify that customer is able to add product to the cart directly from the corresponding product detail page', async ({ page }) => {
        await page.goto('/inventory-item.html?id=4');

        const productDetailPage = new ProductDetailPage(page);
        const isItemCounterVisible = await productDetailPage.cartItemCounter.isVisible();
        const initialItemCount = isItemCounterVisible ? parseInt(await productDetailPage.cartItemCounter.innerText(), 10) : 0;
        await productDetailPage.addProductToCart();
        const expectedItemCount = (initialItemCount + 1).toString();

        await expect(productDetailPage.removeFromCartButton).toHaveText('Remove');
        await expect(productDetailPage.cartItemCounter).toHaveText(expectedItemCount);
    })

    // I can also modify local storage through Playwright so I start the test with the product already added and I just remove it.
    // It's better to do so, but in this small project I decided to prepare the state (adding product to cart) via UI in the removing product from cart test itself
    test('[SLT-9] Verify that customer is able to remove product from the cart directly from the corresponding product detail page', async ({ page }) => {
        await page.goto('/inventory-item.html?id=4')
        
        const productDetailPage = new ProductDetailPage(page);
        await productDetailPage.addProductToCart();

        await productDetailPage.removeProductFromCart();

        await expect(productDetailPage.addToCartButton).toHaveText('Add to cart');

        // All of the test runs are starting from a clean state so we know that after adding and removing an item from the cart, the cart item count will be 0
        // so we are asserting the lack of cart item counter (the badge with the number of items in the cart should not be visible)
        await expect(productDetailPage.cartItemCounter).toBeHidden();
    })

    test('[SLT-10] Verify that the state synchronization is correct between product listing page and product detail page', async({ page }) => {
        const productListingPage = new ProductListingPage(page);
        await productListingPage.addProductToCart();
        await productListingPage.clickOnTheProductName();

        await expect(page).toHaveURL('https://www.saucedemo.com/inventory-item.html?id=4');

        const productDetailPage = new ProductDetailPage(page);
        await expect(productDetailPage.removeFromCartButton).toHaveText('Remove');
    })

    test('[SLT-11] Verify that the state synchronization is correct between product detail page and product listing page', async({ page }) => {
        const productListingPage = new ProductListingPage(page);
        await productListingPage.addProductToCart();

        await page.goto('/inventory-item.html?id=4');
        const productDetailPage = new ProductDetailPage(page);
        await productDetailPage.removeProductFromCart();

        await productDetailPage.clickOnTheBackToProductsButton();

        await expect(page).toHaveURL('https://www.saucedemo.com/inventory.html');
        await expect(productListingPage.addToCartButton).toHaveText('Add to cart');
    })
})