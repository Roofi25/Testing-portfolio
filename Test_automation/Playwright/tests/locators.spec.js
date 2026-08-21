const { test, expect } = require("@playwright/test");


test.describe("Examples", () =>{
    test("Testing selectors", async ({page}) => {
        await page.goto("https://demoqa.com/text-box");
        //await page.pause();
        //Finding a locator by an id
        await page.locator("#userName").fill("Test username");
        //await page.pause();
        //Finding a locator by an attribute (in this case it's the placeholder attribute)
        await page.locator('[placeholder="name@example.com"]').fill("testemail@gmail.com");
        //await page.pause();
        //Finding a locator by an id
        await page.locator("#currentAddress").fill("Test current address");
        //await page.pause();
        //Finding a locator by an id
        await page.locator("#permanentAddress").fill("Test permanent address");
        //await page.pause();
        //Finding a locator using playwright's "has-text" condition. 
        //In this case we are finding a button that has a test "Submit" and then click it.
        await page.locator('button:has-text("Submit")').click();
        await page.pause();
        //Finding a locator using xpath
        //await page.locator('//span[contains(@class,"rct-title") and contains(text(), "Home")]');
    })
})

//xpath
// //input[@placeholder="Full Name"]
// //span[contains(@class,"rct-title") or contains(text(), "Home")]
// //span[contains(@class,"rct-title") and contains(text(), "Home")]