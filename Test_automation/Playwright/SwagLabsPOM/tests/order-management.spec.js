import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { InventoryPage } from '../pages/InventoryPage';

test.describe("Product Selection", () => {
    test.beforeEach(async ({ page }) => {
        await page.goto('/');
        const loginPage = new LoginPage(page);
        await loginPage.login("standard_user", "secret_sauce");
    })

    test('[SLT-6] Verify that customer is able to add product to the cart directly from the inventory page', async ({ page }) => {
        const inventoryPage = new InventoryPage(page);
        const isitemCounterVisible = await inventoryPage.cartItemCounter.isVisible();
        const initialItemCount = isitemCounterVisible ? parseInt(await inventoryPage.cartItemCounter.innerText(), 10) : 0;
        await inventoryPage.addProductToCart();
        const expectedItemCount = (initialItemCount + 1).toString();

        await expect(inventoryPage.removeFromCartButton).toHaveText('Remove');
        await expect(inventoryPage.cartItemCounter).toHaveText(expectedItemCount);
    })

    // I can also modify local storage through Playwright so I start the test with the product already added and I just remove it.
    // It's better to do so, but in this small project I decided to prepare the state (adding product to cart) via UI in the removing product from cart test itself
    test('[SLT-7] Verify that customer is able to remove product from the cart directly from the inventory page', async ({ page }) => {
        const inventoryPage = new InventoryPage(page);
        await inventoryPage.addProductToCart();

        await inventoryPage.removeProductFromCart();

        await expect(inventoryPage.addToCartButton).toHaveText('Add to cart');

        // All of the test runs are starting from a clean state so we know that after adding and removing an item from the cart, the cart item count will be 0
        // so we are asserting the lack of cart item counter (the badge with the number of items in the cart should not be visible)
        await expect(inventoryPage.cartItemCounter).toBeHidden();
    })
})