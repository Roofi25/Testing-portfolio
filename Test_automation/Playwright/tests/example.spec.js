const {test,expect} = require("@playwright/test");

/*
test("First test", async ({page})=>{
    // Here the testing code
    //this one is not necessery anymore, because
    //we specified baseURL in configuration file.
    //await page.goto("https://playwright.dev");
    //this will use the baseURL in the configuration file
    await page.goto("/");
    await page.pause();
    const title = page.locator('.navbar__inner .navbar__title');
    await expect(title).toHaveText('Playwright');
})
*/

test("Second test", async ({page})=>{
    //this one is not necessery anymore, because
    //we specified baseURL in configuration file.
    //await page.goto("https://the-internet.herokuapp.com");
    //this will use the baseURL in the configuration file
    //if we want to overrite the baseURL in configuration file then 
    //we just need to write goto like normal specifying the url
    await page.goto("/");
    //this will redirect to the baseUrl + what is in the text besides /
    //in the case below it will be: https://the-internet.herokuapp.com/checkboxes
    await page.goto("/checkboxes");
    await page.locator('text=Add/Remove Elements').click();
    //Komendy robiące to samo co wyższa linijka:
    //await page.click("text=Add/Remove Elements");
    const element = page.locator('text=Add/Remove Elements');
    //this screenshot will only get the button (locator)
    //await element.screenshot({path: "screenshotforselector.png"})
    //const addElement = page.locator('text=Add Element');
    await element.click();
    //taking a screenshot manually
    //await page.screenshot({path: "screenshot.png", fullPage: true});
    //await addElement.click();
    await page.locator('text=Add Element').click();
})