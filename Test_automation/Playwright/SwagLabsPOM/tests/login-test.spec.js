import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { ProductListingPage } from '../pages/ProductListingPage';

test.describe("Login", () => {

    test.beforeEach(async ({ page }) => {
        await page.goto('/');
    })

    // Array with the list of available usernames - DDT (Data Driven Testing)
    const validUsers = ['standard_user', 'problem_user', 'performance_glitch_user', 'error_user', 'visual_user'];

    // This will execute 6 different tests
    for(const username of validUsers)
    {
        test(`[SLT-1] Verify login using valid credentials for user ${username}`, async ({ page }) => {
            const loginPage = new LoginPage(page);
            await loginPage.login(username, 'secret_sauce');

            const productListingPage = new ProductListingPage(page);
            await expect(productListingPage.cartLink, 'Cart link is not visible!').toBeVisible();
        })
    }

    // Array of objects, used for DDT but when we need more than just one value that is different (for example empty username, empty password and locked_out_user username 
    // have all different outputs, but all of them are login with invalid credentials).
    const invalidDataForLogin = [
        {
            testName: 'InvalidUsername',
            username: 'testUsername',
            password: 'secret_sauce',
            expectedErrorMessage: 'Username and password do not match'
        },
        {
            testName: 'InvalidPassword',
            username: 'standard_user',
            password: 'testPassword',
            expectedErrorMessage: 'Username and password do not match'
        },
        {
            testName: 'EmptyUsername',
            username: '',
            password: 'secret_sauce',
            expectedErrorMessage: 'Username is required'
        },
        {
            testName: 'EmptyPassword',
            username: 'standard_user',
            password: '',
            expectedErrorMessage: 'Password is required'
        },
        {
            testName: 'LockedOutUser',
            username: 'locked_out_user',
            password: 'secret_sauce',
            expectedErrorMessage: 'locked out'
        }
    ]

    for(const invalidData of invalidDataForLogin)
    {
        test(`[SLT-2] Verify that user can't log into the locked out accounts: ${invalidData.testName}`, async({ page }) => {
            const loginPage = new LoginPage(page);
            await loginPage.login(invalidData.username, invalidData.password);

            await expect(loginPage.errorMessage).toContainText(invalidData.expectedErrorMessage);
        })
    }
});