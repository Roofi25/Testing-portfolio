const {test, expect} = require('@playwright/test');

test.describe("Examples", () => {
    test("Testing Assertions", async ({page}) => {
        await page.goto("https://demoqa.com/text-box");
        
         //asserting the url of the page
        await expect(page).toHaveURL('htdtps://demoqa.com/text-box');

        //asserting title of the page
        await expect(page).toHaveTitle('DEMOQA');

        await page.locator("#userName").fill('test');
        await page.locator("#userEmail").fill("test@gmail.com");
        await page.locator('[placeholder="Current Address"]').fill("test");
        await page.locator("#permanentAddress").fill("test");
        await page.locator('button:has-text("Submit")').click();
        await page.pause();

        const name = page.locator('#name');
        const email = page.locator("#email");
        const currentAddress = page.locator("p#currentAddress");
        const permanentAddress = page.locator("p#permanentAddress");

        //asserting the text after submitting a form
        await expect(name).toBeVisible();
        await expect(name).toHaveText("Name:test");

        await expect(email).toBeVisible();
        await expect(email).toHaveText("Email:test@gmail.com");

        await expect(currentAddress).toBeVisible();
        await expect(currentAddress).toHaveText("Current Address :test");

        await expect(permanentAddress).toBeVisible();
        await expect(permanentAddress).toHaveText("Permananet Address :test");
    })
})