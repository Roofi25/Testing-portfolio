exports.ProductListingPage = class ProductListingPage
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
        this.itemImage = page.locator('[data-test="inventory-item-sauce-labs-backpack-img"]');
        this.itemName = page.locator('#item_4_title_link');
        this.itemCard = page.locator('.inventory_item').filter({hasText: "Sauce Labs Backpack"});
        this.itemDescription = this.itemCard.locator('[data-test="inventory-item-desc"]');
        this.itemPrice = this.itemCard.locator('[data-test="inventory-item-price"]');
        this.sortDropdown = page.locator('[data-test="product-sort-container"]');
        this.allProductPrices = page.locator('[data-test="inventory-item-price"]');
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

    async clickOnTheProductName()
    {
        await this.itemName.click();
    }

    async clickOnTheProductImage()
    {
        await this.itemImage.click();
    }

    //optionValue possible values: 
    //az - Name (A to Z)
    //za - Name (Z to A)
    //lohi - Price (low to high)
    //hilo - Price (high to low)
    async sortProductsBy(optionValue)
    {
        await this.sortDropdown.selectOption(optionValue);
    }

    async getAllProductPrices()
    {
        // Fetching the table of strings
        const priceTexts = await this.allProductPrices.allInnerTexts();
        
        // Removing the '$' sign from every price and converting it to the table of floats
        return priceTexts.map(price => parseFloat(price.replace('$', '')));
    }
}