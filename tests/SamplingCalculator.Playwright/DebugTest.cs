using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace SamplingCalculator.Playwright;

[TestFixture]
public class DebugTest : PageTest
{
    private const string BaseUrl = "http://localhost:5173";

    [Test]
    public async Task Debug_CheckPageContent()
    {
        await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.Load });

        // Wait a few seconds for WASM to potentially load
        await Page.WaitForTimeoutAsync(5000);

        var content = await Page.ContentAsync();
        Console.WriteLine("=== PAGE CONTENT START ===");
        Console.WriteLine(content);
        Console.WriteLine("=== PAGE CONTENT END ===");

        // Check if there's any error message
        var errorElement = Page.Locator("#blazor-error-ui");
        var isErrorVisible = await errorElement.IsVisibleAsync();
        Console.WriteLine($"Error UI visible: {isErrorVisible}");

        // Check for h1
        var h1Count = await Page.Locator("h1").CountAsync();
        Console.WriteLine($"H1 count: {h1Count}");

        // Check body content
        var body = await Page.Locator("body").TextContentAsync();
        Console.WriteLine($"Body text: {body}");

        Assert.Pass("Debug info printed");
    }
}
