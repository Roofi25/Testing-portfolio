import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { InventoryPage } from '../pages/InventoryPage';

test.beforeEach(async ({ page }) => {
    await page.goto('/');
})

// Array with the list of available usernames - DDT (Data Driven Testing)
const validUsers = ['standard_user', 'problem_user', 'performance_glitch_user', 'error_user', 'visual_user'];

// This will execute 6 different tests
for(const username of validUsers)
{
    test(`LoginWithValidCredentials for user ${username}`, async ({ page }) => {
        const loginPage = new LoginPage(page);
        await loginPage.login(username, 'secret_sauce');

        const inventoryPage = new InventoryPage(page);
        await expect(inventoryPage.cartLink, 'Cart link is not visible!').toBeVisible();
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
    test(`LoginWithInvalidCredentials: ${invalidData.testName}`, async({ page }) => {
        const loginPage = new LoginPage(page);
        await loginPage.login(invalidData.username, invalidData.password);

        await expect(loginPage.loginErrorMessage).toContainText(invalidData.expectedErrorMessage);
    })
}

// Technically logout should be in different test suite (not a login test suite)
test('CorrectLogout', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.login("standard_user", "secret_sauce");

    const inventoryPage = new InventoryPage(page);
    await inventoryPage.logout();

    await expect(loginPage.loginButton, 'Login button is not visible!').toBeVisible();
})