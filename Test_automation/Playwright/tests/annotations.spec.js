const {test, expect} = require('@playwright/test');


test("First test", async ({page, browserName})=>{
    //This is how we make a test to be skipped, but only on one of the web browsers
    //the 2nd argument is the description
    test.skip(browserName === "firefox", "Working on the firefox fix");
    // Here the testing code
    await page.goto("https://playwright.dev");
    await page.pause();
    const title = page.locator('.navbar__inner .navbar__title');
    await expect(title).toHaveText('Playwright');
})

//'skip' annotation will make this test to be skipped while executing this file (test suite)
test.skip("Second test", async ({page})=>{
    await page.goto("https://the-internet.herokuapp.com");
    await page.locator('text=Add/Remove Elements').click();
    await page.locator('text=Add Element').click();
})


//You can tag the tests using @ in their name. 
//By doing this you can only run the tests 
//of a specific tag in the file (test suite).
//Command in terminal: npx playwright test --grep smoke (on other OS it's @smoke as an argument) 
//will execute the tests only with a tag 'smoke' in a file (test suite).
//Command in terminal: npx playwright test --grep-invert smoke (on the other OS it's @smoke as an argument)
//will execute all of the tests that doesn't have a tag 'smoke' in a file (test suite).
test("Duplicate test1 @smoke", async({page})=>{
    await page.goto("https://the-internet.herokuapp.com");
    await page.locator('text=Add/Remove Elements').click();
    await page.locator('text=Add Element').click();
})

test("Duplicate test2 @regression", async({page})=>{
    await page.goto("https://the-internet.herokuapp.com");
    await page.locator('text=Add/Remove Elements').click();
    await page.locator('text=Add Element').click();
})

test("Duplicate test3 @smoke", async({page})=>{
    await page.goto("https://the-internet.herokuapp.com");
    await page.locator('text=Add/Remove Elements').click();
    await page.locator('text=Add Element').click();
})

test("Duplicate test4 @regression", async({page})=>{
    await page.goto("https://the-internet.herokuapp.com");
    await page.locator('text=Add/Remove Elements').click();
    await page.locator('text=Add Element').click();
})

test("Duplicate test5 @regression-smoke", async({page})=>{
    await page.goto("https://the-internet.herokuapp.com");
    await page.locator('text=Add/Remove Elements').click();
    await page.locator('text=Add Element').click();
})


//'only' annotation will make this test to be the only test executed from this file (test suite)
/*
test.only("Duplicate test", async({page})=>{
    await page.goto("https://the-internet.herokuapp.com");
    await page.locator('text=Add/Remove Elements').click();
    await page.locator('text=Add Element').click();
})
*/