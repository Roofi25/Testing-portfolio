exports.InventoryItemPage = class InventoryItemPage
{
    constructor(page)
    {
        this.page = page;
        this.itemImage = page.locator('.inventory_details_img');
        this.itemName = page.locator('[data-test="inventory-item-name"]');
        this.itemDescription = page.locator('[data-test="inventory-item-desc"]');
        this.itemPrice = page.locator('[data-test="inventory-item-price"]');
        this.addToCartButton = page.locator('#add-to-cart');
        this.removeFromCartButton = page.locator('#remove');
        this.cartItemCounter = page.locator('[data-test="shopping-cart-badge"]');
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