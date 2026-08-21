const {PlaywrightTestConfig} = require('@playwright/test');

const config = {
    //retrying failed test once
    retries: 1,
    //timeout (60000 miliseconds = 60 seconds)
    timeout: 60000,
    //including our custom reporter in the config
    reporter: "./reporter.js",
    //specific ways of playwright are set in 
    //the use property
    use: {
        //this website will be opened by default
        baseURL: "https://the-internet.herokuapp.com",
        //will make headed mode default (will never run in headless mode)
        headless: false,
        //specifying viewport
        viewport: {width: 1200, height: 720},
        //videos
        //video: on - just that will always record a video no matter if the test fails or passes
        //here is the path it creates a video automatically (webm format)
        //D:\Desktop\Kurs_tester_oprogramowania_udemy\Playwright\test-results\tests-example-Second-test-Chrome
        //by default it overrides the previous file every time the test rerun
        //video: "retain-on-failure" - that will record tests only when they fail (only failed tests are recorded).
        //video: "on-first-retry" - if the test fails for the 1st time it will not save the recording, but if it fails 
        //for the second time it records it (helpful in our configuration that retries every test once).
        video: "on-first-retry",
        
        //screenshots
        //screenshot: "on" - will take screenshots on each test. 
        //in this mode screenshots are taken at the end of the test.
        //screenshot: "only-on-failure" - will take screenshots only on failed tests
        //in this mode screenshots are taken at the end of the test.
        screenshot: "only-on-failure",
    },

    //running tests in specific browsers are in the project property
    projects: [
        {
            name: "Chrome",
            use: {browserName: "chromium"}
        },
        {
            name: "Firefox",
            use: {browserName: "firefox"}
        },
        {
            name: "Webkit",
            use: {browserName: "webkit"}
        }
    ]
}

//exporting the config
module.exports = config;

//example of running test using this config (in webkit in this case)
//npx playwright test tests/example.spec.js --config=playwright.config.js --project=Webkit