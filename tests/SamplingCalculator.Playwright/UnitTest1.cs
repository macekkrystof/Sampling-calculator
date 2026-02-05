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
        var focalLength = Page.Locator("#single-focalLength");
        await Expect(focalLength).ToHaveValueAsync("800");

        var pixelSize = Page.Locator("#single-pixelSize");
        await Expect(pixelSize).ToHaveValueAsync("3.76");

        var seeing = Page.Locator("#single-seeing");
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
        var seeing = Page.Locator("#single-seeing");
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

        var focalLength = Page.Locator("#single-focalLength");
        await focalLength.FillAsync("0");
        await focalLength.DispatchEventAsync("input");

        // Should show validation error
        var errorMessage = Page.Locator("#single-focalLength-error");
        await Expect(errorMessage).ToBeVisibleAsync();
        await Expect(errorMessage).ToContainTextAsync("greater than 0");
    }

    [Test]
    public async Task InputValidation_InvalidValues_HidesResults()
    {
        await WaitForBlazorAsync();

        // Set invalid focal length
        var focalLength = Page.Locator("#single-focalLength");
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
        await Page.Locator("#single-binning").SelectOptionAsync("2");

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

        var reducer = Page.Locator("#single-reducer");
        await reducer.FillAsync("1.5");
        await reducer.DispatchEventAsync("input");

        var errorMessage = Page.Locator("#single-reducer-error");
        await Expect(errorMessage).ToBeVisibleAsync();
    }

    [Test]
    public async Task BarlowValidation_Under1_ShowsError()
    {
        await WaitForBlazorAsync();

        var barlow = Page.Locator("#single-barlow");
        await barlow.FillAsync("0.5");
        await barlow.DispatchEventAsync("input");

        var errorMessage = Page.Locator("#single-barlow-error");
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
        var effFocalLabel = Page.Locator("#single-eff-focal-label");
        await Expect(effFocalLabel).ToBeVisibleAsync();
    }

    [Test]
    public async Task Accessibility_ResultsHaveAriaLabels()
    {
        await WaitForBlazorAsync();

        // Results panel should have aria-label
        var resultsPanel = Page.Locator("section[aria-label='Results']");
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
        var pixelScaleValue = Page.Locator("[aria-labelledby='single-pixel-scale-label']");
        await Expect(pixelScaleValue).ToBeVisibleAsync();

        // FOV degrees should be linked to its label
        var fovDegValue = Page.Locator("[aria-labelledby='single-fov-deg-label']");
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

// --- Task 6: Comparison Mode Tests ---

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ComparisonModeTests : PageTest
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
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Sampling Calculator",
            new() { Timeout = 30000 });
    }

    [Test]
    public async Task CompareToggle_IsVisible()
    {
        await WaitForBlazorAsync();

        var toggle = Page.Locator("input[type='checkbox']");
        await Expect(toggle).ToBeVisibleAsync();

        var toggleLabel = Page.Locator(".toggle-label");
        await Expect(toggleLabel).ToHaveTextAsync("Compare mode");
    }

    [Test]
    public async Task CompareToggle_OffByDefault()
    {
        await WaitForBlazorAsync();

        var toggle = Page.Locator("input[type='checkbox']");
        await Expect(toggle).Not.ToBeCheckedAsync();

        // Copy button should not be visible
        var copyBtn = Page.Locator(".copy-btn");
        await Expect(copyBtn).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task CompareMode_EnabledShowsTwoPanels()
    {
        await WaitForBlazorAsync();

        // Enable compare mode
        await Page.Locator("input[type='checkbox']").ClickAsync();

        // Should now see two input panels
        var inputPanels = Page.Locator(".inputs-panel");
        await Expect(inputPanels).ToHaveCountAsync(2);

        // Should see two result panels
        var resultPanels = Page.Locator(".results-panel");
        await Expect(resultPanels).ToHaveCountAsync(2);
    }

    [Test]
    public async Task CompareMode_ShowsSetupAAndSetupBLabels()
    {
        await WaitForBlazorAsync();

        // Enable compare mode
        await Page.Locator("input[type='checkbox']").ClickAsync();

        // Should see "Setup A" and "Setup B" labels
        await Expect(Page.Locator("h2:has-text('Setup A')")).ToBeVisibleAsync();
        await Expect(Page.Locator("h2:has-text('Setup B')")).ToBeVisibleAsync();

        // Should see "Results A" and "Results B" labels
        await Expect(Page.Locator("h2:has-text('Results A')")).ToBeVisibleAsync();
        await Expect(Page.Locator("h2:has-text('Results B')")).ToBeVisibleAsync();
    }

    [Test]
    public async Task CompareMode_CopyButtonVisible()
    {
        await WaitForBlazorAsync();

        // Enable compare mode
        await Page.Locator("input[type='checkbox']").ClickAsync();

        var copyBtn = Page.Locator(".copy-btn");
        await Expect(copyBtn).ToBeVisibleAsync();
        await Expect(copyBtn).ToContainTextAsync("Copy A");
    }

    [Test]
    public async Task CompareMode_CopyAToB_CopiesValues()
    {
        await WaitForBlazorAsync();

        // Change Setup A focal length before enabling compare mode
        var focalLengthA = Page.Locator("#single-focalLength");
        await focalLengthA.FillAsync("1200");
        await focalLengthA.DispatchEventAsync("input");

        // Enable compare mode
        await Page.Locator("input[type='checkbox']").ClickAsync();

        // Setup B should have same value (copied when entering compare mode)
        var focalLengthB = Page.Locator("#b-focalLength");
        await Expect(focalLengthB).ToHaveValueAsync("1200");
    }

    [Test]
    public async Task CompareMode_IndependentInputs()
    {
        await WaitForBlazorAsync();

        // Enable compare mode
        await Page.Locator("input[type='checkbox']").ClickAsync();

        // Change Setup A
        var focalLengthA = Page.Locator("#a-focalLength");
        await focalLengthA.FillAsync("1500");
        await focalLengthA.DispatchEventAsync("input");

        // Setup B should still have the original value
        var focalLengthB = Page.Locator("#b-focalLength");
        await Expect(focalLengthB).ToHaveValueAsync("800");
    }

    [Test]
    public async Task CompareMode_CopyButton_UpdatesSetupB()
    {
        await WaitForBlazorAsync();

        // Enable compare mode
        await Page.Locator("input[type='checkbox']").ClickAsync();

        // Change Setup A to different values
        var focalLengthA = Page.Locator("#a-focalLength");
        await focalLengthA.FillAsync("2000");
        await focalLengthA.DispatchEventAsync("input");

        var binningA = Page.Locator("#a-binning");
        await binningA.SelectOptionAsync("2");

        // Click Copy A to B
        await Page.Locator(".copy-btn").ClickAsync();

        // Verify Setup B now matches
        var focalLengthB = Page.Locator("#b-focalLength");
        await Expect(focalLengthB).ToHaveValueAsync("2000");

        var binningB = Page.Locator("#b-binning");
        await Expect(binningB).ToHaveValueAsync("2");
    }

    [Test]
    public async Task CompareMode_BothResultsUpdate()
    {
        await WaitForBlazorAsync();

        // Enable compare mode
        await Page.Locator("input[type='checkbox']").ClickAsync();

        // Both results should have status badges
        var statusBadges = Page.Locator(".status-badge");
        await Expect(statusBadges).ToHaveCountAsync(2);

        // Change Setup A binning to see different result
        await Page.Locator("#a-binning").SelectOptionAsync("4");

        // Results A should still show (pixel scale changed)
        var resultsA = Page.Locator(".setup-a .results-panel .primary-metric-value").First;
        await Expect(resultsA).ToBeVisibleAsync();
    }

    [Test]
    public async Task CompareMode_DisablingReturnsSinglePanel()
    {
        await WaitForBlazorAsync();

        // Enable compare mode
        await Page.Locator("input[type='checkbox']").ClickAsync();

        // Verify two panels exist
        await Expect(Page.Locator(".inputs-panel")).ToHaveCountAsync(2);

        // Disable compare mode
        await Page.Locator("input[type='checkbox']").ClickAsync();

        // Should be back to single panel
        await Expect(Page.Locator(".inputs-panel")).ToHaveCountAsync(1);
        await Expect(Page.Locator(".results-panel")).ToHaveCountAsync(1);
    }

    [Test]
    public async Task CompareMode_Desktop_SideBySideLayout()
    {
        // Set large desktop viewport
        await Page.SetViewportSizeAsync(1400, 900);
        await WaitForBlazorAsync();

        // Enable compare mode
        await Page.Locator("input[type='checkbox']").ClickAsync();

        // The two setup columns should be side by side
        var setupA = Page.Locator(".setup-a");
        var setupB = Page.Locator(".setup-b");

        var boxA = await setupA.BoundingBoxAsync();
        var boxB = await setupB.BoundingBoxAsync();

        Assert.That(boxA, Is.Not.Null);
        Assert.That(boxB, Is.Not.Null);

        // Setup B should be to the right of Setup A
        Assert.That(boxB!.X, Is.GreaterThan(boxA!.X), "Setup B should be to the right of Setup A on desktop");
    }

    [Test]
    public async Task CompareMode_Mobile_StackedLayout()
    {
        // Set mobile viewport
        await Page.SetViewportSizeAsync(375, 800);
        await WaitForBlazorAsync();

        // Enable compare mode
        await Page.Locator("input[type='checkbox']").ClickAsync();

        // The two setup columns should be stacked
        var setupA = Page.Locator(".setup-a");
        var setupB = Page.Locator(".setup-b");

        var boxA = await setupA.BoundingBoxAsync();
        var boxB = await setupB.BoundingBoxAsync();

        Assert.That(boxA, Is.Not.Null);
        Assert.That(boxB, Is.Not.Null);

        // Setup B should be below Setup A
        Assert.That(boxB!.Y, Is.GreaterThan(boxA!.Y), "Setup B should be below Setup A on mobile");
    }

    [Test]
    public async Task CompareMode_SetupAHasAccentBorder()
    {
        await WaitForBlazorAsync();

        // Enable compare mode
        await Page.Locator("input[type='checkbox']").ClickAsync();

        // Setup A should exist with its border styling
        var setupA = Page.Locator(".setup-a .inputs-panel");
        await Expect(setupA).ToBeVisibleAsync();
    }

    [Test]
    public async Task CompareMode_SetupBHasGreenBorder()
    {
        await WaitForBlazorAsync();

        // Enable compare mode
        await Page.Locator("input[type='checkbox']").ClickAsync();

        // Setup B should exist with its border styling
        var setupB = Page.Locator(".setup-b .inputs-panel");
        await Expect(setupB).ToBeVisibleAsync();
    }
}
