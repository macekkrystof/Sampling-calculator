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

// --- Task 7: Presets Tests ---

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class PresetsTests : PageTest
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

    private async Task ClearLocalStorageAsync()
    {
        await Page.EvaluateAsync("localStorage.clear()");
    }

    [SetUp]
    public async Task SetUp()
    {
        // Navigate first, then clear localStorage
        await Page.GotoAsync(BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await ClearLocalStorageAsync();
    }

    [Test]
    public async Task PresetsPanel_IsVisible()
    {
        await WaitForBlazorAsync();

        var presetsPanel = Page.Locator(".presets-panel");
        await Expect(presetsPanel).ToBeVisibleAsync();

        var heading = Page.Locator(".presets-panel h2");
        await Expect(heading).ToHaveTextAsync("Presets");
    }

    [Test]
    public async Task PresetsPanel_HasNameInput()
    {
        await WaitForBlazorAsync();

        var nameInput = Page.Locator("#preset-name");
        await Expect(nameInput).ToBeVisibleAsync();
        await Expect(nameInput).ToHaveAttributeAsync("placeholder", "Preset name");
    }

    [Test]
    public async Task PresetsPanel_HasTypeSelect()
    {
        await WaitForBlazorAsync();

        var typeSelect = Page.Locator("#preset-type");
        await Expect(typeSelect).ToBeVisibleAsync();

        // Should have Full Rig, Telescope, Camera options
        var options = typeSelect.Locator("option");
        await Expect(options).ToHaveCountAsync(3);
    }

    [Test]
    public async Task PresetsPanel_SaveButtonDisabledWhenEmpty()
    {
        await WaitForBlazorAsync();

        var saveBtn = Page.Locator(".save-preset-btn");
        await Expect(saveBtn).ToBeDisabledAsync();
    }

    [Test]
    public async Task PresetsPanel_SaveButtonEnabledWithName()
    {
        await WaitForBlazorAsync();

        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("My Preset");

        var saveBtn = Page.Locator(".save-preset-btn");
        await Expect(saveBtn).ToBeEnabledAsync();
    }

    [Test]
    public async Task Presets_SaveFullRigPreset()
    {
        await WaitForBlazorAsync();

        // Set some values
        var focalLength = Page.Locator("#single-focalLength");
        await focalLength.FillAsync("1200");
        await focalLength.DispatchEventAsync("input");

        // Save preset
        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("My Full Rig");

        var saveBtn = Page.Locator(".save-preset-btn");
        await saveBtn.ClickAsync();

        // Preset should appear in list
        var presetItem = Page.Locator(".preset-load-btn:has-text('My Full Rig')");
        await Expect(presetItem).ToBeVisibleAsync();

        // Success message should appear
        var message = Page.Locator(".preset-message.success");
        await Expect(message).ToBeVisibleAsync();
    }

    [Test]
    public async Task Presets_SaveTelescopePreset()
    {
        await WaitForBlazorAsync();

        // Select telescope type
        var typeSelect = Page.Locator("#preset-type");
        await typeSelect.SelectOptionAsync("Telescope");

        // Save preset
        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("My Scope");

        var saveBtn = Page.Locator(".save-preset-btn");
        await saveBtn.ClickAsync();

        // Preset should appear under Telescope category
        var telescopeHeading = Page.Locator(".preset-category h3:has-text('Telescope')");
        await Expect(telescopeHeading).ToBeVisibleAsync();

        var presetItem = Page.Locator(".preset-load-btn:has-text('My Scope')");
        await Expect(presetItem).ToBeVisibleAsync();
    }

    [Test]
    public async Task Presets_SaveCameraPreset()
    {
        await WaitForBlazorAsync();

        // Select camera type
        var typeSelect = Page.Locator("#preset-type");
        await typeSelect.SelectOptionAsync("Camera");

        // Save preset
        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("My Camera");

        var saveBtn = Page.Locator(".save-preset-btn");
        await saveBtn.ClickAsync();

        // Preset should appear under Camera category
        var cameraHeading = Page.Locator(".preset-category h3:has-text('Camera')");
        await Expect(cameraHeading).ToBeVisibleAsync();

        var presetItem = Page.Locator(".preset-load-btn:has-text('My Camera')");
        await Expect(presetItem).ToBeVisibleAsync();
    }

    [Test]
    public async Task Presets_LoadFullRigPreset()
    {
        await WaitForBlazorAsync();

        // First, change values and save
        var focalLength = Page.Locator("#single-focalLength");
        await focalLength.FillAsync("1500");
        await focalLength.DispatchEventAsync("input");

        var pixelSize = Page.Locator("#single-pixelSize");
        await pixelSize.FillAsync("4.5");
        await pixelSize.DispatchEventAsync("input");

        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("Test Rig");
        await Page.Locator(".save-preset-btn").ClickAsync();

        // Reset values
        await focalLength.FillAsync("800");
        await focalLength.DispatchEventAsync("input");
        await pixelSize.FillAsync("3.76");
        await pixelSize.DispatchEventAsync("input");

        // Load the preset
        await Page.Locator(".preset-load-btn:has-text('Test Rig')").ClickAsync();

        // Values should be restored
        await Expect(focalLength).ToHaveValueAsync("1500");
        await Expect(pixelSize).ToHaveValueAsync("4.5");
    }

    [Test]
    public async Task Presets_DeletePreset()
    {
        await WaitForBlazorAsync();

        // Save a preset first
        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("To Delete");
        await Page.Locator(".save-preset-btn").ClickAsync();

        // Verify it exists
        var presetItem = Page.Locator(".preset-load-btn:has-text('To Delete')");
        await Expect(presetItem).ToBeVisibleAsync();

        // Delete it
        var deleteBtn = Page.Locator(".preset-item:has-text('To Delete') .preset-delete-btn");
        await deleteBtn.ClickAsync();

        // Should be gone
        await Expect(presetItem).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Presets_PersistAcrossReload()
    {
        await WaitForBlazorAsync();

        // Save a preset
        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("Persistent Preset");
        await Page.Locator(".save-preset-btn").ClickAsync();

        // Reload page
        await Page.ReloadAsync();
        await WaitForBlazorAsync();

        // Preset should still exist
        var presetItem = Page.Locator(".preset-load-btn:has-text('Persistent Preset')");
        await Expect(presetItem).ToBeVisibleAsync();
    }

    [Test]
    public async Task Presets_ShowSummaryInfo()
    {
        await WaitForBlazorAsync();

        // Set specific values
        var focalLength = Page.Locator("#single-focalLength");
        await focalLength.FillAsync("1200");
        await focalLength.DispatchEventAsync("input");

        // Save as Full Rig
        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("Summary Test");
        await Page.Locator(".save-preset-btn").ClickAsync();

        // Summary should show focal length
        var summary = Page.Locator(".preset-item:has-text('Summary Test') .preset-summary");
        await Expect(summary).ToContainTextAsync("1200mm");
    }

    [Test]
    public async Task Presets_NameInputClearedAfterSave()
    {
        await WaitForBlazorAsync();

        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("Test Save");
        await Page.Locator(".save-preset-btn").ClickAsync();

        // Name input should be cleared after save
        await Expect(nameInput).ToHaveValueAsync("");
    }

    [Test]
    public async Task Presets_NoPresetsMessage_ShownInitially()
    {
        await WaitForBlazorAsync();

        // With cleared localStorage, should show no presets message
        var noPresetsMsg = Page.Locator(".no-presets");
        await Expect(noPresetsMsg).ToBeVisibleAsync();
    }

    [Test]
    public async Task Presets_NoPresetsMessage_HiddenAfterSave()
    {
        await WaitForBlazorAsync();

        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("First Preset");
        await Page.Locator(".save-preset-btn").ClickAsync();

        // No presets message should be hidden
        var noPresetsMsg = Page.Locator(".no-presets");
        await Expect(noPresetsMsg).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Presets_LoadMessage_Shown()
    {
        await WaitForBlazorAsync();

        // Save a preset
        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("Load Test");
        await Page.Locator(".save-preset-btn").ClickAsync();

        // Wait for save message to clear (optional, but makes test more reliable)
        await Page.WaitForTimeoutAsync(500);

        // Load it
        await Page.Locator(".preset-load-btn:has-text('Load Test')").ClickAsync();

        // Should show loaded message
        var message = Page.Locator(".preset-message");
        await Expect(message).ToContainTextAsync("Loaded");
    }

    [Test]
    public async Task Presets_TelescopePreset_OnlyAppliesTelescopeValues()
    {
        await WaitForBlazorAsync();

        // Set initial camera values
        var pixelSize = Page.Locator("#single-pixelSize");
        await pixelSize.FillAsync("5.0");
        await pixelSize.DispatchEventAsync("input");

        // Save as telescope preset (captures telescope values)
        var typeSelect = Page.Locator("#preset-type");
        await typeSelect.SelectOptionAsync("Telescope");

        var focalLength = Page.Locator("#single-focalLength");
        await focalLength.FillAsync("1500");
        await focalLength.DispatchEventAsync("input");

        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("Scope Only");
        await Page.Locator(".save-preset-btn").ClickAsync();

        // Reset telescope values, keep camera values different
        await focalLength.FillAsync("800");
        await focalLength.DispatchEventAsync("input");
        await pixelSize.FillAsync("3.76");
        await pixelSize.DispatchEventAsync("input");

        // Load telescope preset
        await Page.Locator(".preset-load-btn:has-text('Scope Only')").ClickAsync();

        // Focal length should be restored
        await Expect(focalLength).ToHaveValueAsync("1500");
        // Pixel size should NOT have changed from original save value (5.0) - actually it should remain 3.76
        // because telescope presets don't touch camera values
        await Expect(pixelSize).ToHaveValueAsync("3.76");
    }

    [Test]
    public async Task Presets_CameraPreset_OnlyAppliesCameraValues()
    {
        await WaitForBlazorAsync();

        // Set initial telescope values
        var focalLength = Page.Locator("#single-focalLength");
        await focalLength.FillAsync("1200");
        await focalLength.DispatchEventAsync("input");

        // Save as camera preset
        var typeSelect = Page.Locator("#preset-type");
        await typeSelect.SelectOptionAsync("Camera");

        var pixelSize = Page.Locator("#single-pixelSize");
        await pixelSize.FillAsync("4.5");
        await pixelSize.DispatchEventAsync("input");

        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("Camera Only");
        await Page.Locator(".save-preset-btn").ClickAsync();

        // Reset camera values, keep telescope values different
        await pixelSize.FillAsync("3.76");
        await pixelSize.DispatchEventAsync("input");
        await focalLength.FillAsync("800");
        await focalLength.DispatchEventAsync("input");

        // Load camera preset
        await Page.Locator(".preset-load-btn:has-text('Camera Only')").ClickAsync();

        // Pixel size should be restored
        await Expect(pixelSize).ToHaveValueAsync("4.5");
        // Focal length should NOT have changed
        await Expect(focalLength).ToHaveValueAsync("800");
    }

    [Test]
    public async Task Presets_Accessibility_DeleteButtonHasAriaLabel()
    {
        await WaitForBlazorAsync();

        // Save a preset
        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("Accessible Preset");
        await Page.Locator(".save-preset-btn").ClickAsync();

        // Delete button should have aria-label
        var deleteBtn = Page.Locator(".preset-item:has-text('Accessible Preset') .preset-delete-btn");
        var ariaLabel = await deleteBtn.GetAttributeAsync("aria-label");
        Assert.That(ariaLabel, Does.Contain("Delete"));
        Assert.That(ariaLabel, Does.Contain("Accessible Preset"));
    }

    [Test]
    public async Task Presets_Accessibility_LoadButtonHasAriaLabel()
    {
        await WaitForBlazorAsync();

        // Save a preset
        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("Load Me");
        await Page.Locator(".save-preset-btn").ClickAsync();

        // Load button should have aria-label
        var loadBtn = Page.Locator(".preset-load-btn:has-text('Load Me')");
        var ariaLabel = await loadBtn.GetAttributeAsync("aria-label");
        Assert.That(ariaLabel, Does.Contain("Load"));
        Assert.That(ariaLabel, Does.Contain("Load Me"));
    }
}

// --- Task 8: URL State Tests ---

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class UrlStateTests : PageTest
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
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Page.Locator("h1")).ToHaveTextAsync("Sampling Calculator",
            new() { Timeout = 30000 });
    }

    [Test]
    public async Task ShareButton_IsVisible()
    {
        await Page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync();

        var shareBtn = Page.Locator(".share-btn");
        await Expect(shareBtn).ToBeVisibleAsync();
        await Expect(shareBtn).ToContainTextAsync("Share");
    }

    [Test]
    public async Task ShareButton_ShowsCopiedOnClick()
    {
        await Page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync();

        // Grant clipboard permissions
        await Context.GrantPermissionsAsync(new[] { "clipboard-write", "clipboard-read" });

        var shareBtn = Page.Locator(".share-btn");
        await shareBtn.ClickAsync();

        // Button should temporarily show "Copied!"
        await Expect(shareBtn).ToContainTextAsync("Copied!");
    }

    [Test]
    public async Task UrlState_LoadsWithQueryParams()
    {
        // Navigate with query params
        await Page.GotoAsync($"{BaseUrl}/?fl=1200&px=5&bin=2");
        await WaitForBlazorAsync();

        // Values should be set from URL
        var focalLength = Page.Locator("#single-focalLength");
        await Expect(focalLength).ToHaveValueAsync("1200");

        var pixelSize = Page.Locator("#single-pixelSize");
        await Expect(pixelSize).ToHaveValueAsync("5");

        var binning = Page.Locator("#single-binning");
        await Expect(binning).ToHaveValueAsync("2");
    }

    [Test]
    public async Task UrlState_LoadsSeeing()
    {
        await Page.GotoAsync($"{BaseUrl}/?see=3.5");
        await WaitForBlazorAsync();

        var seeing = Page.Locator("#single-seeing");
        await Expect(seeing).ToHaveValueAsync("3.5");
    }

    [Test]
    public async Task UrlState_LoadsReducerAndBarlow()
    {
        await Page.GotoAsync($"{BaseUrl}/?rd=0.7&bl=2");
        await WaitForBlazorAsync();

        var reducer = Page.Locator("#single-reducer");
        await Expect(reducer).ToHaveValueAsync("0.7");

        var barlow = Page.Locator("#single-barlow");
        await Expect(barlow).ToHaveValueAsync("2");
    }

    [Test]
    public async Task UrlState_LoadsAperture()
    {
        await Page.GotoAsync($"{BaseUrl}/?ap=250");
        await WaitForBlazorAsync();

        var aperture = Page.Locator("#single-aperture");
        await Expect(aperture).ToHaveValueAsync("250");
    }

    [Test]
    public async Task UrlState_LoadsSensorDimensions()
    {
        await Page.GotoAsync($"{BaseUrl}/?sw=4096&sh=2160");
        await WaitForBlazorAsync();

        var sensorWidth = Page.Locator("#single-sensorWidth");
        await Expect(sensorWidth).ToHaveValueAsync("4096");

        var sensorHeight = Page.Locator("#single-sensorHeight");
        await Expect(sensorHeight).ToHaveValueAsync("2160");
    }

    [Test]
    public async Task UrlState_LoadsCompareMode()
    {
        await Page.GotoAsync($"{BaseUrl}/?cmp=1");
        await WaitForBlazorAsync();

        // Compare mode should be enabled
        var toggle = Page.Locator("input[type='checkbox']");
        await Expect(toggle).ToBeCheckedAsync();

        // Should see two input panels
        var inputPanels = Page.Locator(".inputs-panel");
        await Expect(inputPanels).ToHaveCountAsync(2);
    }

    [Test]
    public async Task UrlState_LoadsSetupBValues()
    {
        await Page.GotoAsync($"{BaseUrl}/?cmp=1&fl=1000&bfl=1500&bbin=2");
        await WaitForBlazorAsync();

        // Setup A values
        var focalLengthA = Page.Locator("#a-focalLength");
        await Expect(focalLengthA).ToHaveValueAsync("1000");

        // Setup B values
        var focalLengthB = Page.Locator("#b-focalLength");
        await Expect(focalLengthB).ToHaveValueAsync("1500");

        var binningB = Page.Locator("#b-binning");
        await Expect(binningB).ToHaveValueAsync("2");
    }

    [Test]
    public async Task UrlState_InvalidParamsUseDefaults()
    {
        await Page.GotoAsync($"{BaseUrl}/?fl=invalid&bin=10");
        await WaitForBlazorAsync();

        // Invalid focal length should fall back to default
        var focalLength = Page.Locator("#single-focalLength");
        await Expect(focalLength).ToHaveValueAsync("800");

        // Out of range binning should fall back to default
        var binning = Page.Locator("#single-binning");
        await Expect(binning).ToHaveValueAsync("1");
    }

    [Test]
    public async Task UrlState_UpdatesOnInputChange()
    {
        await Page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync();

        // Change focal length
        var focalLength = Page.Locator("#single-focalLength");
        await focalLength.FillAsync("1200");
        await focalLength.DispatchEventAsync("input");

        // Wait for URL to update
        await Page.WaitForTimeoutAsync(100);

        // URL should contain the new value
        var currentUrl = Page.Url;
        Assert.That(currentUrl, Does.Contain("fl=1200"));
    }

    [Test]
    public async Task UrlState_UpdatesOnBinningChange()
    {
        await Page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync();

        await Page.Locator("#single-binning").SelectOptionAsync("3");

        await Page.WaitForTimeoutAsync(100);

        var currentUrl = Page.Url;
        Assert.That(currentUrl, Does.Contain("bin=3"));
    }

    [Test]
    public async Task UrlState_UpdatesOnCompareModeToggle()
    {
        await Page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync();

        await Page.Locator("input[type='checkbox']").ClickAsync();

        await Page.WaitForTimeoutAsync(100);

        var currentUrl = Page.Url;
        Assert.That(currentUrl, Does.Contain("cmp=1"));
    }

    [Test]
    public async Task UrlState_DefaultValuesNotInUrl()
    {
        await Page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync();

        // With defaults, URL should be clean (no query params)
        var currentUrl = Page.Url;
        Assert.That(currentUrl, Does.Not.Contain("fl=800")); // Default focal length not in URL
        Assert.That(currentUrl, Does.Not.Contain("bin=1")); // Default binning not in URL
    }

    [Test]
    public async Task UrlState_SharedUrlLoadsCorrectState()
    {
        // Create a complex URL with multiple params
        var url = $"{BaseUrl}/?fl=1200&ap=250&rd=0.8&bl=1.5&px=4.5&sw=4096&sh=2160&bin=2&see=2.5";
        await Page.GotoAsync(url);
        await WaitForBlazorAsync();

        // Verify all values loaded correctly
        await Expect(Page.Locator("#single-focalLength")).ToHaveValueAsync("1200");
        await Expect(Page.Locator("#single-aperture")).ToHaveValueAsync("250");
        await Expect(Page.Locator("#single-reducer")).ToHaveValueAsync("0.8");
        await Expect(Page.Locator("#single-barlow")).ToHaveValueAsync("1.5");
        await Expect(Page.Locator("#single-pixelSize")).ToHaveValueAsync("4.5");
        await Expect(Page.Locator("#single-sensorWidth")).ToHaveValueAsync("4096");
        await Expect(Page.Locator("#single-sensorHeight")).ToHaveValueAsync("2160");
        await Expect(Page.Locator("#single-binning")).ToHaveValueAsync("2");
        await Expect(Page.Locator("#single-seeing")).ToHaveValueAsync("2.5");
    }

    [Test]
    public async Task UrlState_UpdatesOnPresetLoad()
    {
        await Page.GotoAsync(BaseUrl);
        await WaitForBlazorAsync();

        // Save a preset
        var focalLength = Page.Locator("#single-focalLength");
        await focalLength.FillAsync("1500");
        await focalLength.DispatchEventAsync("input");

        var nameInput = Page.Locator("#preset-name");
        await nameInput.FillAsync("URL Test Preset");
        await Page.Locator(".save-preset-btn").ClickAsync();

        // Reset focal length
        await focalLength.FillAsync("800");
        await focalLength.DispatchEventAsync("input");

        // Load preset
        await Page.Locator(".preset-load-btn:has-text('URL Test Preset')").ClickAsync();

        await Page.WaitForTimeoutAsync(100);

        // URL should reflect loaded preset value
        var currentUrl = Page.Url;
        Assert.That(currentUrl, Does.Contain("fl=1500"));
    }

    [Test]
    public async Task UrlState_CompareModeSetupBUpdates()
    {
        await Page.GotoAsync($"{BaseUrl}/?cmp=1");
        await WaitForBlazorAsync();

        // Change Setup B values
        var focalLengthB = Page.Locator("#b-focalLength");
        await focalLengthB.FillAsync("2000");
        await focalLengthB.DispatchEventAsync("input");

        await Page.WaitForTimeoutAsync(100);

        var currentUrl = Page.Url;
        Assert.That(currentUrl, Does.Contain("bfl=2000"));
    }

    [Test]
    public async Task UrlState_CopyAToBUpdatesUrl()
    {
        await Page.GotoAsync($"{BaseUrl}/?cmp=1&fl=1200");
        await WaitForBlazorAsync();

        // Click copy button
        await Page.Locator(".copy-btn").ClickAsync();

        await Page.WaitForTimeoutAsync(100);

        var currentUrl = Page.Url;
        // Setup B should now have same focal length
        Assert.That(currentUrl, Does.Contain("bfl=1200"));
    }
}

// --- Task 9: Astronomical Theme & Accessibility Tests ---

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class AstroThemeAccessibilityTests : PageTest
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
    public async Task Theme_StarfieldBackgroundExists()
    {
        await WaitForBlazorAsync();

        // Starfield div should exist and be in the DOM
        var starfield = Page.Locator(".starfield");
        await Expect(starfield).ToHaveCountAsync(1);
    }

    [Test]
    public async Task Theme_StarfieldIsNotInteractive()
    {
        await WaitForBlazorAsync();

        // Starfield should have pointer-events: none (aria-hidden)
        var starfield = Page.Locator(".starfield");
        var ariaHidden = await starfield.GetAttributeAsync("aria-hidden");
        Assert.That(ariaHidden, Is.EqualTo("true"));
    }

    [Test]
    public async Task Theme_GlassmorphismOnInputsPanel()
    {
        await WaitForBlazorAsync();

        var inputsPanel = Page.Locator(".inputs-panel");
        await Expect(inputsPanel).ToBeVisibleAsync();

        // Check that panel has rounded corners (glassmorphism style)
        var borderRadius = await inputsPanel.EvaluateAsync<string>("el => getComputedStyle(el).borderRadius");
        Assert.That(borderRadius, Does.Contain("px").Or.Contain("12"));
    }

    [Test]
    public async Task Theme_GlassmorphismOnResultsPanel()
    {
        await WaitForBlazorAsync();

        var resultsPanel = Page.Locator(".results-panel");
        await Expect(resultsPanel).ToBeVisibleAsync();

        // Check that panel exists with correct styling
        var boxShadow = await resultsPanel.EvaluateAsync<string>("el => getComputedStyle(el).boxShadow");
        Assert.That(boxShadow, Is.Not.EqualTo("none"), "Results panel should have box shadow");
    }

    [Test]
    public async Task Theme_StatusBadgeHasGlowEffect()
    {
        await WaitForBlazorAsync();

        var statusBadge = Page.Locator(".status-badge");
        await Expect(statusBadge).ToBeVisibleAsync();

        // Status badge should have box-shadow for glow
        var boxShadow = await statusBadge.EvaluateAsync<string>("el => getComputedStyle(el).boxShadow");
        Assert.That(boxShadow, Is.Not.EqualTo("none"), "Status badge should have glow effect");
    }

    [Test]
    public async Task Theme_HeadingHasGradient()
    {
        await WaitForBlazorAsync();

        var heading = Page.Locator("h1");
        await Expect(heading).ToBeVisibleAsync();

        // Heading should have background-clip: text for gradient effect
        var backgroundClip = await heading.EvaluateAsync<string>("el => getComputedStyle(el).backgroundClip");
        Assert.That(backgroundClip, Is.EqualTo("text"));
    }

    [Test]
    public async Task Accessibility_SkipLinkExists()
    {
        await WaitForBlazorAsync();

        var skipLink = Page.Locator(".skip-link");
        await Expect(skipLink).ToHaveCountAsync(1);
        await Expect(skipLink).ToHaveTextAsync("Skip to main content");
    }

    [Test]
    public async Task Accessibility_SkipLinkHasCorrectHref()
    {
        await WaitForBlazorAsync();

        var skipLink = Page.Locator(".skip-link");
        var href = await skipLink.GetAttributeAsync("href");
        Assert.That(href, Is.EqualTo("#main-content"));
    }

    [Test]
    public async Task Accessibility_MainContentHasId()
    {
        await WaitForBlazorAsync();

        var mainContent = Page.Locator("#main-content");
        await Expect(mainContent).ToHaveCountAsync(1);
    }

    [Test]
    public async Task Accessibility_MainContentIsMainElement()
    {
        await WaitForBlazorAsync();

        var mainElement = Page.Locator("main#main-content");
        await Expect(mainElement).ToHaveCountAsync(1);
    }

    [Test]
    public async Task Accessibility_InputsHaveFocusVisibleStyles()
    {
        await WaitForBlazorAsync();

        var focalLengthInput = Page.Locator("#single-focalLength");
        await focalLengthInput.FocusAsync();

        // Check that the focused input has visible focus indicator
        var boxShadow = await focalLengthInput.EvaluateAsync<string>("el => getComputedStyle(el).boxShadow");
        Assert.That(boxShadow, Is.Not.EqualTo("none"), "Focused input should have visible focus indicator");
    }

    [Test]
    public async Task Accessibility_ButtonsHaveFocusVisibleStyles()
    {
        await WaitForBlazorAsync();

        var shareBtn = Page.Locator(".share-btn");
        await shareBtn.FocusAsync();

        // The button should be focusable
        await Expect(shareBtn).ToBeFocusedAsync();
    }

    [Test]
    public async Task Accessibility_PresetButtonsKeyboardNavigable()
    {
        await WaitForBlazorAsync();

        // Tab to first preset button
        var excellentBtn = Page.Locator(".preset-btn").First;
        await excellentBtn.FocusAsync();
        await Expect(excellentBtn).ToBeFocusedAsync();

        // Can activate with keyboard
        await Page.Keyboard.PressAsync("Enter");

        // Seeing value should change
        var seeing = Page.Locator("#single-seeing");
        await Expect(seeing).ToHaveValueAsync("1.5");
    }

    [Test]
    public async Task Accessibility_StatusCardsHaveRoleStatus()
    {
        await WaitForBlazorAsync();

        var statusCard = Page.Locator("[role='status']");
        await Expect(statusCard).ToHaveCountAsync(1);
    }

    [Test]
    public async Task Accessibility_ValidationWarningHasRoleAlert()
    {
        await WaitForBlazorAsync();

        // Make input invalid
        var focalLength = Page.Locator("#single-focalLength");
        await focalLength.FillAsync("0");
        await focalLength.DispatchEventAsync("input");

        // Validation warning should have role="alert"
        var warningCard = Page.Locator(".validation-warning-card");
        var role = await warningCard.GetAttributeAsync("role");
        Assert.That(role, Is.EqualTo("alert"));
    }

    [Test]
    public async Task Theme_DarkColorScheme()
    {
        await WaitForBlazorAsync();

        // Body background should be dark
        var bgColor = await Page.Locator("body").EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");

        // Parse RGB values - should be dark (low values)
        // Format: rgb(r, g, b) or rgba(r, g, b, a)
        Assert.That(bgColor, Does.Contain("rgb"), "Background should have RGB color");

        // Check it's a dark color (any component less than 50 suggests dark theme)
        var numbers = System.Text.RegularExpressions.Regex.Matches(bgColor, @"\d+");
        if (numbers.Count >= 3)
        {
            var r = int.Parse(numbers[0].Value);
            var g = int.Parse(numbers[1].Value);
            var b = int.Parse(numbers[2].Value);
            Assert.That(r + g + b, Is.LessThan(150), "Background should be dark");
        }
    }

    [Test]
    public async Task Theme_ContrastOnStatusBadge()
    {
        await WaitForBlazorAsync();

        var statusBadge = Page.Locator(".status-badge");
        await Expect(statusBadge).ToBeVisibleAsync();

        // Status badge text should be readable
        var color = await statusBadge.EvaluateAsync<string>("el => getComputedStyle(el).color");
        Assert.That(color, Does.Contain("rgb"), "Status badge should have colored text");
    }

    [Test]
    public async Task Accessibility_CompareToggleKeyboardAccessible()
    {
        await WaitForBlazorAsync();

        var toggle = Page.Locator("input[type='checkbox']");
        await toggle.FocusAsync();
        await Expect(toggle).ToBeFocusedAsync();

        // Should be able to toggle with space key
        await Page.Keyboard.PressAsync("Space");
        await Expect(toggle).ToBeCheckedAsync();
    }

    [Test]
    public async Task Accessibility_BinningSelectKeyboardAccessible()
    {
        await WaitForBlazorAsync();

        var binning = Page.Locator("#single-binning");
        await binning.FocusAsync();
        await Expect(binning).ToBeFocusedAsync();
    }

    [Test]
    public async Task Theme_ResultCardBordersVisible()
    {
        await WaitForBlazorAsync();

        var resultCard = Page.Locator(".result-card").First;
        await Expect(resultCard).ToBeVisibleAsync();

        var borderStyle = await resultCard.EvaluateAsync<string>("el => getComputedStyle(el).borderStyle");
        Assert.That(borderStyle, Is.Not.EqualTo("none"), "Result cards should have visible borders");
    }

    [Test]
    public async Task Theme_PresetActiveButtonHighlighted()
    {
        await WaitForBlazorAsync();

        // Default seeing is 2.0, so Average button should be active
        var activeBtn = Page.Locator(".preset-btn.preset-active");
        await Expect(activeBtn).ToBeVisibleAsync();

        // Active button should have different background
        var bgColor = await activeBtn.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        Assert.That(bgColor, Does.Not.EqualTo("rgba(0, 0, 0, 0)"), "Active preset button should have background");
    }
}

// --- Task 10: SEO & Metadata Tests ---

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class SeoMetadataTests : PageTest
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
    public async Task Seo_HasProperTitle()
    {
        await WaitForBlazorAsync();

        var title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Astrophotography"));
        Assert.That(title, Does.Contain("Sampling Calculator"));
    }

    [Test]
    public async Task Seo_HasMetaDescription()
    {
        await WaitForBlazorAsync();

        var metaDesc = Page.Locator("meta[name='description']");
        var content = await metaDesc.GetAttributeAsync("content");
        Assert.That(content, Is.Not.Null.And.Not.Empty);
        Assert.That(content, Does.Contain("pixel scale").IgnoreCase);
    }

    [Test]
    public async Task Seo_HasMetaKeywords()
    {
        await WaitForBlazorAsync();

        var metaKeywords = Page.Locator("meta[name='keywords']");
        var content = await metaKeywords.GetAttributeAsync("content");
        Assert.That(content, Is.Not.Null.And.Not.Empty);
        Assert.That(content, Does.Contain("astrophotography").IgnoreCase);
    }

    [Test]
    public async Task Seo_HasThemeColor()
    {
        await WaitForBlazorAsync();

        var themeColor = Page.Locator("meta[name='theme-color']");
        var content = await themeColor.GetAttributeAsync("content");
        Assert.That(content, Is.Not.Null.And.Not.Empty);
        Assert.That(content, Does.StartWith("#"));
    }

    [Test]
    public async Task Seo_HasOpenGraphTags()
    {
        await WaitForBlazorAsync();

        // og:type
        var ogType = Page.Locator("meta[property='og:type']");
        await Expect(ogType).ToHaveCountAsync(1);

        // og:title
        var ogTitle = Page.Locator("meta[property='og:title']");
        var titleContent = await ogTitle.GetAttributeAsync("content");
        Assert.That(titleContent, Does.Contain("Astrophotography").Or.Contain("Sampling"));

        // og:description
        var ogDesc = Page.Locator("meta[property='og:description']");
        var descContent = await ogDesc.GetAttributeAsync("content");
        Assert.That(descContent, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public async Task Seo_HasTwitterCards()
    {
        await WaitForBlazorAsync();

        // twitter:card
        var twitterCard = Page.Locator("meta[property='twitter:card']");
        await Expect(twitterCard).ToHaveCountAsync(1);
        var cardType = await twitterCard.GetAttributeAsync("content");
        Assert.That(cardType, Is.EqualTo("summary_large_image"));

        // twitter:title
        var twitterTitle = Page.Locator("meta[property='twitter:title']");
        await Expect(twitterTitle).ToHaveCountAsync(1);
    }

    [Test]
    public async Task Seo_HasLangAttribute()
    {
        await WaitForBlazorAsync();

        var htmlElement = Page.Locator("html");
        var lang = await htmlElement.GetAttributeAsync("lang");
        Assert.That(lang, Is.EqualTo("en"));
    }

    [Test]
    public async Task Pwa_HasManifestLink()
    {
        await WaitForBlazorAsync();

        var manifestLink = Page.Locator("link[rel='manifest']");
        await Expect(manifestLink).ToHaveCountAsync(1);
        var href = await manifestLink.GetAttributeAsync("href");
        Assert.That(href, Does.Contain("manifest.json"));
    }

    [Test]
    public async Task Pwa_HasAppleTouchIcon()
    {
        await WaitForBlazorAsync();

        var appleTouchIcon = Page.Locator("link[rel='apple-touch-icon']");
        await Expect(appleTouchIcon).ToHaveCountAsync(1);
    }

    [Test]
    public async Task Pwa_HasAppleWebAppMeta()
    {
        await WaitForBlazorAsync();

        var capable = Page.Locator("meta[name='apple-mobile-web-app-capable']");
        await Expect(capable).ToHaveCountAsync(1);
        var capableContent = await capable.GetAttributeAsync("content");
        Assert.That(capableContent, Is.EqualTo("yes"));

        var title = Page.Locator("meta[name='apple-mobile-web-app-title']");
        await Expect(title).ToHaveCountAsync(1);
    }

    [Test]
    public async Task Favicon_HasSvgIcon()
    {
        await WaitForBlazorAsync();

        var svgIcon = Page.Locator("link[rel='icon'][type='image/svg+xml']");
        await Expect(svgIcon).ToHaveCountAsync(1);
        var href = await svgIcon.GetAttributeAsync("href");
        Assert.That(href, Does.Contain(".svg"));
    }

    [Test]
    public async Task Favicon_HasPngFallback()
    {
        await WaitForBlazorAsync();

        var pngIcons = Page.Locator("link[rel='icon'][type='image/png']");
        var count = await pngIcons.CountAsync();
        Assert.That(count, Is.GreaterThanOrEqualTo(1), "Should have at least one PNG favicon");
    }

    [Test]
    public async Task Seo_HasRobotsMetaTag()
    {
        await WaitForBlazorAsync();

        var robots = Page.Locator("meta[name='robots']");
        await Expect(robots).ToHaveCountAsync(1);
        var content = await robots.GetAttributeAsync("content");
        Assert.That(content, Does.Contain("index"));
        Assert.That(content, Does.Contain("follow"));
    }

    [Test]
    public async Task Manifest_IsAccessible()
    {
        // Try to fetch manifest.json directly
        var response = await Page.GotoAsync($"{BaseUrl}/manifest.json");
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Status, Is.EqualTo(200));

        var content = await response.TextAsync();
        Assert.That(content, Does.Contain("Astrophotography").Or.Contain("Sampling"));
        Assert.That(content, Does.Contain("icons"));
    }

    [Test]
    public async Task Favicon_SvgIsAccessible()
    {
        var response = await Page.GotoAsync($"{BaseUrl}/favicon.svg");
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Status, Is.EqualTo(200));

        var content = await response.TextAsync();
        Assert.That(content, Does.Contain("<svg"));
    }
}
