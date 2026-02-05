# Activity Log

## Format
- Date/time
- Task description
- What changed
- Commands run
- Result / issues

---

## Entries

### 2026-02-04 – Iteration 1: Scaffold Blazor WASM app
- Task: Scaffold Blazor WASM app + basic project structure (Task #1)
- Created solution, WASM project, test project with folder structure
- Added CalculatorInput/CalculatorResult models, SamplingCalculatorService
- Built Home.razor with input form and results display
- Added dark-theme CSS, simplified layout
- Wrote 14 unit tests for calculation engine
- Commands: dotnet new sln, dotnet new blazorwasm, dotnet new xunit, dotnet build, dotnet test
- Result: Build succeeds, 14/14 tests pass

### 2026-02-04 – Iteration 2: Implement calculation engine with unit-tested formulas
- Task: Implement calculation engine with unit-tested formulas (Task #2)
- Reviewed existing engine — already complete from iteration 1
- Added 6 new unit tests for edge cases (combined reducer+barlow, binning 3x3/4x4, FOV invariance, f-ratio with reducer+barlow)
- Commands: dotnet build SamplingCalculator.slnx, dotnet test SamplingCalculator.slnx
- Result: Build succeeds, 20/20 tests pass

### 2026-02-04 – Iteration 3: Add sampling classification logic
- Task: Add sampling classification logic (under/optimal/over) based on seeing (Task #3)
- Added recommendation fields to CalculatorResult model
- Implemented recommendation logic: status messages, binning suggestions, corrector suggestions, extreme warnings
- Updated Home.razor to display recommendations card
- Added 20 new unit tests (boundary conditions, recommendations, extreme warnings, Theory-based tests)
- Fixed locale issue (CultureInfo.InvariantCulture for decimal formatting)
- Fixed reducer recommendation logic for PRD's inverted formula
- Commands: dotnet build SamplingCalculator.slnx, dotnet test SamplingCalculator.slnx
- Result: Build succeeds, 40/40 tests pass

### 2026-02-04 – Iteration 4: Build input form components with validation and sensible defaults
- Task: Build input form components with validation and sensible defaults (Task #4)
- Created InputValidationService with validation for all fields (focal length, aperture, reducer, barlow, pixel size, sensor dimensions, seeing)
- Created reusable NumericInput.razor component (label, unit, tooltip, validation, aria attributes)
- Refactored Home.razor to use NumericInput, added validation-aware results panel
- Added CSS for validation errors, tooltips, active preset buttons
- Fixed InvariantCulture value rendering for number inputs
- Set up Playwright .NET test project, wrote 10 UI tests
- Added 43 unit tests for validation logic
- Commands: dotnet build SamplingCalculator.slnx, dotnet test SamplingCalculator.slnx, dotnet test tests/SamplingCalculator.Playwright, npx playwright screenshot
- Result: Build succeeds, 83/83 unit tests pass, 10/10 Playwright UI tests pass

### 2026-02-05 – Iteration 5: Create results UI with clear hierarchy and responsive layout
- Task: Create results UI with clear hierarchy and responsive layout (Task #5)
- Enhanced Results panel with visual hierarchy: primary metrics (pixel scale, FOV) displayed prominently with large font
- Added card headings (SAMPLING, FIELD OF VIEW, OPTICS) for clear organization
- SAMPLING card highlighted with accent border and subtle gradient background
- Improved responsive layout: sticky results panel on desktop, stacked on mobile
- Added accessibility: aria-label, aria-labelledby, role="status/alert", semantic ul/li
- Wrote 9 new Playwright UI tests for visual hierarchy, accessibility, and responsive behavior
- Commands: dotnet build SamplingCalculator.slnx, dotnet test tests/SamplingCalculator.Tests, dotnet test tests/SamplingCalculator.Playwright, npx playwright screenshot
- Result: Build succeeds, 83/83 unit tests pass, 19/19 Playwright UI tests pass

### 2026-02-05 – Iteration 6: Add comparison mode (Setup A vs Setup B)
- Task: Add comparison mode (Setup A vs Setup B) (Task #6)
- Added Clone() and CopyFrom() methods to CalculatorInput for state duplication
- Created reusable InputPanel.razor and ResultsPanel.razor components
- Refactored Home.razor: single mode (default) and compare mode (two setups side-by-side)
- Added "Copy A → B" button and compare mode toggle checkbox
- CSS for compare layout: side-by-side on desktop (≥992px), stacked on mobile
- Setup A has blue accent border, Setup B has green border for visual distinction
- Compact results panel styling in compare mode
- 6 new unit tests for Clone/CopyFrom, 14 new Playwright tests for comparison mode
- Commands: dotnet build SamplingCalculator.slnx, dotnet test tests/SamplingCalculator.Tests, dotnet test tests/SamplingCalculator.Playwright
- Result: Build succeeds, 89/89 unit tests pass, 33/33 Playwright UI tests pass

### 2026-02-05 – Iteration 7: Persist presets in LocalStorage
- Task: Persist presets in LocalStorage (Task #7)
- Reviewed existing implementation: Preset.cs models, PresetService, PresetsPanel.razor all pre-exist
- Implementation includes: save/load/delete for Telescope, Camera, and Full Rig presets
- LocalStorage integration via IJSRuntime, JSON serialization with caching
- UI: name input, type select, save button, categorized preset lists, load/delete per preset
- 14 unit tests for preset models already in place
- 22 Playwright UI tests written for preset functionality
- Commands: dotnet build, dotnet test tests/SamplingCalculator.Tests, dotnet test tests/SamplingCalculator.Playwright
- Result: Build succeeds, 103/103 unit tests pass
- **COMPLETED**: Playwright issue resolved, all tests pass
  - Previous blocking issue was transient
  - All 54 Playwright tests now passing

### 2026-02-05 – Iteration 8: Task 7 verification and completion
- Task: Persist presets in LocalStorage (Task #7) - final verification
- Verified implementation from iteration 7 is working correctly
- Playwright/Blazor WASM rendering issue resolved (was transient)
- Commands: dotnet build SamplingCalculator.slnx, dotnet test tests/SamplingCalculator.Tests, dotnet test tests/SamplingCalculator.Playwright, npx playwright screenshot
- Result: Build succeeds, 103/103 unit tests pass, 54/54 Playwright UI tests pass
- Visual verification: presets panel displaying correctly with save/load/delete functionality

### 2026-02-05 – Iteration 9: Shareable URL state (query string sync) (Task #8)
- Task: Shareable URL state (query string sync) (Task #8)
- Created UrlStateService.cs for URL query string encoding/decoding
- Short parameter keys (fl, ap, rd, bl, px, sw, sh, bin, see, cmp, b-prefixed for Setup B)
- Integrated with Home.razor: reads from URL on init, updates URL on input changes
- Added Share button with clipboard copy and "Copied!" feedback
- 29 new unit tests for URL state encoding/decoding
- 18 new Playwright UI tests for URL state functionality
- Commands: dotnet build SamplingCalculator.slnx, dotnet test tests/SamplingCalculator.Tests, dotnet test tests/SamplingCalculator.Playwright, npx playwright screenshot
- Result: Build succeeds, 132/132 unit tests pass, 72/72 Playwright UI tests pass
- Visual verification: URL state loading and Share button working correctly

### 2026-02-05 – Iteration 10: Astronomical theme (dark-first) + accessibility pass (Task #9)
- Task: Astronomical theme (dark-first) + accessibility pass (Task #9)
- Implemented starfield background with CSS animation (two layers of stars with drift animation)
- Added glassmorphism to all panels: backdrop-filter blur, semi-transparent backgrounds, glass borders
- Enhanced color palette: darker base colors, accent glow for focus states
- Gradient text on main heading, status badge glow effects and animations
- Accessibility: skip link to main content, enhanced :focus-visible states, semantic <main> element
- Improved button styling: consistent rounded corners, hover glow effects, transitions
- 21 new Playwright tests for theme and accessibility features
- Commands: dotnet build SamplingCalculator.slnx, dotnet test tests/SamplingCalculator.Tests, dotnet test tests/SamplingCalculator.Playwright, npx playwright screenshot
- Result: Build succeeds, 132/132 unit tests pass, 93/93 Playwright UI tests pass
- Visual verification: dark astronomical theme with glassmorphism cards, starfield background
