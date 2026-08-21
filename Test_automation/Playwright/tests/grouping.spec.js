const { test, expect } = require('@playwright/test');


//Creating a group of tests .describe()
//parallel - will make these tests run in parallel mode. 
//that will make the tests run at the same time, not one after the other.
test.describe.parallel("Smoke tests", () => {
    //beforeEach hook - will do these commands before each of the tests
    test.beforeEach(async ({page}) =>{
        //base url in config file
        await page.goto("/");
    })

    //afterEach hook - will do these commands after each of the tests
    //object 'testInfo' provides info about test that is being executed (this hook is being executed on each test)
    //it will dinamically write in a console the name of the test(title) and status that it was finished in.
    test.afterEach(async ({page}, testInfo) => {
        console.log(`Test "${testInfo.title}" has finished with status: ${testInfo.status}`);
    })

    test("Duplicate test1", async ({ page }) => {
        await page.locator('text=Add/Remove Elements').click();
        await page.locator('text=Add Element').click();
    })

    test("Duplicate test2", async ({ page }) => {
        await page.locator('text=Add/Remove Elements').click();
        await page.locator('text=Add Element').click();
    })

    test("Duplicate test3", async ({ page }) => {
        await page.locator('text=Add/Remove Elements').click();
        await page.locator('text=Add Element').click();
    })

    test("Duplicate test4", async ({ page }) => {
        await page.locator('text=Add/Remove Elements').click();
        await page.locator('text=Add Element').click();
    })
})

