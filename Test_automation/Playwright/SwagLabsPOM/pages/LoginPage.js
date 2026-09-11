exports.LoginPage = class LoginPage
{
    constructor (page)
    {
        this.page = page;

        this.usernameInput = page.locator('#user-name');
        this.passwordInput = page.locator('#password');
        this.loginButton = page.locator('#login-button');
        this.loginErrorMessage = page.locator('[data-test="error"]')
    }

    // Thanks to the Playwright Auto-waiting feature, there is no need for explicit waits
    async login(username, password)
    {
        await this.usernameInput.fill(username);
        await this.passwordInput.fill(password);
        await this.loginButton.click();
    }
}