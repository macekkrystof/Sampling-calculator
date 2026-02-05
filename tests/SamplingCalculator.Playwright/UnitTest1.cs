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

        // Pixel scale should be displayed prominently
        var pixelScaleMetric = Page.Locator(".primary-metric-value").First;
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

        // Get initial pixel scale text from the primary metric
        var pixelScaleValue = Page.Locator(".primary-metric-value").First;
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

    [Test]
    public async Task Results_ShowPrimaryPixelScaleWithLargeValue()
    {
        await WaitForBlazorAsync();

        // Primary pixel scale should be displayed prominently
        var pixelScaleMetric = Page.Locator(".primary-metric-value").First;
        await Expect(pixelScaleMetric).ToBeVisibleAsync();
        await Expect(pixelScaleMetric).ToContainTextAsync("″/px");
    }

    [Test]
    public async Task Results_ShowsCardHeadings()
    {
        await WaitForBlazorAsync();

        // Result cards should have headings for hierarchy
        var cardHeadings = Page.Locator(".card-heading");
        var count = await cardHeadings.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(3), "Expected at least 3 card headings (Sampling, FOV, Optics)");
    }

    [Test]
    public async Task Results_SamplingCardIsHighlighted()
    {
        await WaitForBlazorAsync();

        // Primary metrics card should have highlighted styling
        var primaryCard = Page.Locator(".primary-metrics-card");
        await Expect(primaryCard).ToBeVisibleAsync();
    }

    [Test]
    public async Task Results_FOVDisplayedInArcminutes()
    {
        await WaitForBlazorAsync();

        // FOV primary metric should show arcminutes
        var fovMetric = Page.Locator(".fov-metric .primary-metric-value");
        await Expect(fovMetric).ToBeVisibleAsync();
        await Expect(fovMetric).ToContainTextAsync("′");
    }

    [Test]
    public async Task Results_OpticsCardShowsEffectiveFocal()
    {
        await WaitForBlazorAsync();

        // Optics card should show effective focal length
        var opticsHeading = Page.Locator(".card-heading:has-text('Optics')");
        await Expect(opticsHeading).ToBeVisibleAsync();

        // Effective focal length metric should be present
        var effFocalLabel = Page.Locator("#eff-focal-label");
        await Expect(effFocalLabel).ToBeVisibleAsync();
    }

    [Test]
    public async Task Accessibility_ResultsHaveAriaLabels()
    {
        await WaitForBlazorAsync();

        // Results panel should have aria-label
        var resultsPanel = Page.Locator("section[aria-label='Calculation results']");
        await Expect(resultsPanel).ToBeVisibleAsync();

        // Status badge should have aria-label
        var statusBadge = Page.Locator(".status-badge");
        var ariaLabel = await statusBadge.GetAttributeAsync("aria-label");
        Assert.That(ariaLabel, Does.Contain("Sampling status"));
    }

    [Test]
    public async Task Accessibility_MetricsHaveAriaLabelledBy()
    {
        await WaitForBlazorAsync();

        // Pixel scale value should be linked to its label
        var pixelScaleValue = Page.Locator("[aria-labelledby='pixel-scale-label']");
        await Expect(pixelScaleValue).ToBeVisibleAsync();

        // FOV degrees should be linked to its label
        var fovDegValue = Page.Locator("[aria-labelledby='fov-deg-label']");
        await Expect(fovDegValue).ToBeVisibleAsync();
    }

    [Test]
    public async Task Responsive_DesktopLayout_TwoColumns()
    {
        // Set desktop viewport
        await Page.SetViewportSizeAsync(1200, 800);
        await WaitForBlazorAsync();

        // Both panels should be visible side by side
        var inputsPanel = Page.Locator(".inputs-panel");
        var resultsPanel = Page.Locator(".results-panel");

        var inputsBox = await inputsPanel.BoundingBoxAsync();
        var resultsBox = await resultsPanel.BoundingBoxAsync();

        Assert.That(inputsBox, Is.Not.Null);
        Assert.That(resultsBox, Is.Not.Null);

        // Results should be to the right of inputs (Y positions should overlap, X positions should not)
        Assert.That(resultsBox!.X, Is.GreaterThan(inputsBox!.X), "Results panel should be to the right of inputs panel on desktop");
    }

    [Test]
    public async Task Responsive_MobileLayout_SingleColumn()
    {
        // Set mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        await WaitForBlazorAsync();

        // Both panels should be stacked vertically
        var inputsPanel = Page.Locator(".inputs-panel");
        var resultsPanel = Page.Locator(".results-panel");

        var inputsBox = await inputsPanel.BoundingBoxAsync();
        var resultsBox = await resultsPanel.BoundingBoxAsync();

        Assert.That(inputsBox, Is.Not.Null);
        Assert.That(resultsBox, Is.Not.Null);

        // Results should be below inputs (Y position should be greater)
        Assert.That(resultsBox!.Y, Is.GreaterThan(inputsBox!.Y), "Results panel should be below inputs panel on mobile");
    }
}
