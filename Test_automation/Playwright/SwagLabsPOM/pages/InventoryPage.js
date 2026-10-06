exports.InventoryPage = class InventoryPage
{
    constructor(page)
    {
        this.page = page;

        this.cartLink = page.locator('[data-test="shopping-cart-link"]');
        this.hamburgerIcon = page.locator('#react-burger-menu-btn');
        this.logoutLink = page.locator('[data-test="logout-sidebar-link"]');
        this.addToCartButton = page.locator('[data-test="add-to-cart-sauce-labs-backpack"]');
        this.removeFromCartButton = page.locator('[data-test="remove-sauce-labs-backpack"]');
        this.cartItemCounter = page.locator('[data-test="shopping-cart-badge"]');
    }

    // Thanks to the Playwright Auto-waiting feature, there is no need for explicit waits
    async logout()
    {
        await this.hamburgerIcon.click();
        await this.logoutLink.click();
    }

    async addProductToCart()
    {
        await this.addToCartButton.click();
    }

    async removeProductFromCart()
    {
        await this.removeFromCartButton.click();
    }
}