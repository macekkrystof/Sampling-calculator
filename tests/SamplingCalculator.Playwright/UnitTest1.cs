using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace SamplingCalculator.Playwright;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class InputFormTests : PageTest
{
    private const string BaseUrl = "http://localhost:5173";

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
        };
    }

    private async Task WaitForBlazorAsync()
    {
        await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        // Wait for Blazor to render the app (h1 appears after WASM loads)
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Sampling Calculator",
            new() { Timeout = 30000 });
    }

    [Test]
    public async Task HomePage_LoadsWithDefaults()
    {
        await WaitForBlazorAsync();

        // Check default values are populated
        var focalLength = Page.Locator("#focalLength");
        await Expect(focalLength).ToHaveValueAsync("800");

        var pixelSize = Page.Locator("#pixelSize");
        await Expect(pixelSize).ToHaveValueAsync("3.76");

        var seeing = Page.Locator("#seeing");
        await Expect(seeing).ToHaveValueAsync("2");
    }

    [Test]
    public async Task Results_ShowPixelScaleAndStatus()
    {
        await WaitForBlazorAsync();

        // Results should be visible with defaults
        var statusBadge = Page.Locator(".status-badge");
        await Expect(statusBadge).ToBeVisibleAsync();

        // Pixel scale should be displayed
        var pixelScaleMetric = Page.Locator(".metric-value").First;
        await Expect(pixelScaleMetric).ToContainTextAsync("/px");
    }

    [Test]
    public async Task SeeingPresets_ChangeValue()
    {
        await WaitForBlazorAsync();

        // Click "Excellent" preset
        await Page.Locator(".preset-btn").First.ClickAsync();
        var seeing = Page.Locator("#seeing");
        await Expect(seeing).ToHaveValueAsync("1.5");

        // Click "Poor" preset
        await Page.Locator(".preset-btn").Last.ClickAsync();
        await Expect(seeing).ToHaveValueAsync("3");
    }

    [Test]
    public async Task SeeingPreset_ActiveButtonHighlighted()
    {
        await WaitForBlazorAsync();

        // Default seeing is 2.0, so "Average" should be active
        var averageBtn = Page.Locator(".preset-btn").Nth(1);
        await Expect(averageBtn).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("preset-active"));
    }

    [Test]
    public async Task InputValidation_InvalidFocalLength_ShowsError()
    {
        await WaitForBlazorAsync();

        var focalLength = Page.Locator("#focalLength");
        await focalLength.FillAsync("0");
        await focalLength.DispatchEventAsync("input");

        // Should show validation error
        var errorMessage = Page.Locator("#focalLength-error");
        await Expect(errorMessage).ToBeVisibleAsync();
        await Expect(errorMessage).ToContainTextAsync("greater than 0");
    }

    [Test]
    public async Task InputValidation_InvalidValues_HidesResults()
    {
        await WaitForBlazorAsync();

        // Set invalid focal length
        var focalLength = Page.Locator("#focalLength");
        await focalLength.FillAsync("0");
        await focalLength.DispatchEventAsync("input");

        // Results should be replaced with validation warning
        var warningCard = Page.Locator(".validation-warning-card");
        await Expect(warningCard).ToBeVisibleAsync();
    }

    [Test]
    public async Task BinningSelect_ChangesResults()
    {
        await WaitForBlazorAsync();

        // Get initial pixel scale text
        var pixelScaleValue = Page.Locator(".metric-value").First;
        var initialText = await pixelScaleValue.TextContentAsync();

        // Change binning to 2x2
        await Page.Locator("#binning").SelectOptionAsync("2");

        // Pixel scale should have changed
        await Expect(pixelScaleValue).Not.ToHaveTextAsync(initialText!);
    }

    [Test]
    public async Task TooltipIcons_ArePresent()
    {
        await WaitForBlazorAsync();

        // Wait for tooltip icons to render
        var tooltips = Page.Locator(".tooltip-icon");
        await Expect(tooltips.First).ToBeVisibleAsync();

        var count = await tooltips.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(5), "Expected at least 5 tooltip icons");

        // Check first tooltip has a title
        var firstTooltipTitle = await tooltips.First.GetAttributeAsync("title");
        Assert.That(firstTooltipTitle, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task ReducerValidation_Over1_ShowsError()
    {
        await WaitForBlazorAsync();

        var reducer = Page.Locator("#reducer");
        await reducer.FillAsync("1.5");
        await reducer.DispatchEventAsync("input");

        var errorMessage = Page.Locator("#reducer-error");
        await Expect(errorMessage).ToBeVisibleAsync();
    }

    [Test]
    public async Task BarlowValidation_Under1_ShowsError()
    {
        await WaitForBlazorAsync();

        var barlow = Page.Locator("#barlow");
        await barlow.FillAsync("0.5");
        await barlow.DispatchEventAsync("input");

        var errorMessage = Page.Locator("#barlow-error");
        await Expect(errorMessage).ToBeVisibleAsync();
    }
}
