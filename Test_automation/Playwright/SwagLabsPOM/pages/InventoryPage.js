exports.InventoryPage = class InventoryPage
{
    constructor(page)
    {
        this.page = page;

        this.cartLink = page.locator('[data-test="shopping-cart-link"]');
        this.hamburgerIcon = page.locator('#react-burger-menu-btn');
        this.logoutLink = page.locator('[data-test="logout-sidebar-link"]');
    }

    // Thanks to the Playwright Auto-waiting feature, there is no need for explicit waits
    async logout()
    {
        await this.hamburgerIcon.click();
        await this.logoutLink.click();
    }
}